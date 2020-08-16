import numpy as np
import pandas as pd
from pathlib import Path
from datetime import date
from copy import copy
import logging
from abc import ABC, abstractmethod

import a_star
from nicpy import nic_misc
nic_misc.logging_setup(Path.cwd(), date.today())
_logger = logging.getLogger('pathfinding_logger')

SQRT2 = np.sqrt(2)



# Abstract 'Node' class   ->     Place   /  Square      / Hex
# Abstract 'Graph' class  ->     Network /  SquareGrid  / HexGrid
class Node(ABC):

    _neighbours_costs = {}

    def __init__(self, label, neighbours_costs, parent=None, accessible=True):
        self.check_label(label)
        self.label = label

        self.set_neighbours_costs(neighbours_costs)

        if (parent is not None) and (not isinstance(parent, type(self))): raise Exception('Parent node must be of the same type as the child node.')
        self.parent = parent

        if not isinstance(accessible, bool): raise Exception('\'accessible\' parameter must be a boolean.')
        self.accessible = accessible

        self.g, self.h, self.f = 0, 0, 0

    def __eq__(self, other): return self.label == other.label

    def __repr__(self): return 'Node {} - g={}, h={} f={}'.format(self.label, self.g, self.h, self.f)

    # Defining less than for purposes of heap queue
    def __lt__(self, other): return self.f < other.f

    # Defining greater than for purposes of heap queue
    def __gt__(self, other): return self.f > other.f

    def set_neighbours_costs(self, neighbours_costs):
        if not isinstance(neighbours_costs, dict): raise Exception('Neighbours must be specified as a dict of labels:costs.')
        for neighbour in neighbours_costs.keys():
            self.check_label(neighbour)
        self._neighbours_costs = neighbours_costs

    @abstractmethod
    def check_label(self, label): pass

    @abstractmethod
    def get_cost_to_leave(self, neighbour): pass

    @abstractmethod
    def get_heuristic_dist(self, other, heuristic_type): pass

class Place(Node):

    def check_label(self, label):
        if not isinstance(label, str): raise Exception('PathListNode label must be a non-empty nonstring.')
        if label == '': raise Exception('PathListNode label must be a non-empty string.')

    def get_cost_to_leave(self, neighbour): return self._neighbours_costs[neighbour.label] # / speed TODO: modified by agent speed

    def get_heuristic_dist(self, other, heuristic_type): return 0

class Square(Node):

    def check_label(self, label):
        tuple_error_msg = 'Grid label (coordinates) must be a tuple of length 2.'
        if not isinstance(label, tuple): raise Exception(tuple_error_msg)
        if len(label) != 2: raise Exception(tuple_error_msg)

    def get_cost_to_leave(self, neighbour):
        distance = SQRT2 if neighbour.is_diagonal_neighbour(self) else 1
        return self._neighbours_costs[neighbour.label] * distance # / speed TODO: modified by agent speed

    def get_heuristic_dist(self, other, heuristic_type = 'euclidian'):
        return nic_misc.distance(heuristic_type, self.label, other.label)

    def is_diagonal_neighbour(self, other):
        return (self.label[0]-other.label[0], self.label[1]-other.label[1]) in SquareGrid.diagonal_coord_deltas


class Graph(ABC):

    # nodes = {}      # dict with label:node
    start = None    # start node label
    end   = None    # end node label
    solution = []

    @abstractmethod
    def load_graph(self, filename): pass

    @abstractmethod
    def find_neighbours(self, label): pass

    @abstractmethod
    def is_accessible(self, label): pass

    @abstractmethod
    def create_node(self, label, parent=None): pass

    @abstractmethod
    def solve(self, save_history=False): pass

    @abstractmethod
    def set_start(self, label): pass

    @abstractmethod
    def set_end(self, label): pass

    @abstractmethod
    def check_graph(self): pass

    @abstractmethod
    def _check_label_setting(self): pass

class Network(Graph):

    start = ()
    end = ()
    network = []

    def __init__(self, excel_network_filename):

        if excel_network_filename:
            try:
                self.load_graph(excel_network_filename)
            except:
                raise Exception('Failed to load maze with filename {}.'.format(excel_network_filename))
        else:
            raise Exception('Must specify a file from which to load a network representation.')
            # TODO: random network generator self.generate_random_network()
        self.all_labels = set(self.network[0].to_list() + self.network[1].to_list())

    def load_graph(self, filename):
        filepath = str(Path('excel_networks') / filename)
        self.network = pd.read_excel(filepath, header=None)
        invalid = self.check_network_invalid()
        if invalid:
            _logger.error('Network invalid: {} node pairing has multiple specifications.'.format(invalid))
            raise Exception()
        _logger.info('Loaded network from {}.'.format(filename))

    # TODO: what if two disjoint sections?
    def check_network_invalid(self):
        # Check that there are no duplicate entries (same pair of nodes)
        for i in range(self.network.shape[0]-1):
            row = self.network.iloc[i]
            for j in range(i+1, self.network.shape[0]):
                subsequent_row = self.network.iloc[j]
                if (row[0]==subsequent_row[0] and row[1]==subsequent_row[1]) or (row[0]==subsequent_row[1] and row[1]==subsequent_row[0]):
                    return [row[0], row[1]]
        return []

        # TODO: check types

    def find_neighbours(self, label):
        if label not in self.all_labels: raise Exception('Label not in network.')
        neighbours_costs = {}
        neighbour_rows1 = self.network[self.network[0] == label]
        for i, row in neighbour_rows1.iterrows():
            if row[1] in neighbours_costs.keys(): raise Exception('Invalid network detected: multiple connections from {} to {}.'.format(label, row[1]))
            neighbours_costs[row[1]] = row[2]
        neighbour_rows2 = self.network[self.network[1] == label]
        for i, row in neighbour_rows2.iterrows():
            if row[0] in neighbours_costs.keys(): raise Exception('Invalid network detected: multiple connections from {} to {}.'.format(label, row[0]))
            neighbours_costs[row[0]] = row[2]
        return neighbours_costs

    def is_accessible(self, label): return True

    def create_node(self, label, parent=None):
        return Place(label, self.find_neighbours(label), parent=parent, accessible=self.is_accessible(label))

    def solve(self, save_history=False):
        if self.check_graph():
            self.solution = a_star.run_a_star(self, heuristic_type=None, save_history=save_history)
            if isinstance(self.solution, int):
                _logger.info('Unable to solve the network after {} iterations.'.format(self.solution))
            else:
                _logger.info('Successfully solved the Network with {} steps.'.format(len(self.solution)-1))

    def set_start(self, label):
        if self._check_label_setting(label): self.start = label

    def set_end(self, label):
        if self._check_label_setting(label): self.end = label

    def check_graph(self):
        if not isinstance(self.network, pd.DataFrame): raise Exception('No network specified.')
        if self.network.empty: raise Exception('The specified network is empty.')
        if not self.start and not self.end: raise Exception('No start or end labels specified.')
        if not self.start: raise Exception('No start label specified.')
        if not self.end: raise Exception('No end label specified.')
        if self.start == self.end: raise Exception('The start and end labels are identical.')
        return True

    def _check_label_setting(self, label):
        if not isinstance(label, str): raise Exception('PathListNode label must be a non-empty nonstring.')
        if label == '': raise Exception('PathListNode label must be a non-empty string.')
        if not self.is_accessible(label) or label in [self.start, self.end]:
            raise Exception('Cannot place a start or end on an inaccessible node or an existing start or end.')
        if label not in self.network[0].to_list() and label not in self.network[1].to_list():
            raise Exception('Label {} not in network.'.format(label))
        return True

class SquareGrid(Graph):

    start = ()
    end = ()
    dimensions = ()
    maze_array = np.array([])
    maze_array_solved = np.array([])

    # Adjacent squares to search
    straight_coords_deltas = [(-1, 0), (0, -1), (0, 1), (1, 0)]
    diagonal_coord_deltas = [(-1, -1), (1, 1), (-1, 1), (1, -1)]

    def __init__(self, heuristic_type, excel_maze_filename=None, diagonality=False):

        self.heuristic_type = heuristic_type

        if not isinstance(diagonality, bool): raise Exception('\'diagonality\' parameter must be a boolean.')
        self._diagonality = diagonality

        if excel_maze_filename:
            try: self.load_graph(excel_maze_filename)
            except: raise Exception('Failed to load maze with filename {}'.format(excel_maze_filename))
        else: self.generate_random_maze(10, 10, 0.5)

    def load_graph(self, filename):
        filepath = str(Path('excel_mazes')/filename)
        self.maze_array = np.asarray(pd.read_excel(filepath, header=None))
        self.dimensions = self.maze_array.shape
        # TODO: detect blanks and make them walls (0)
        _logger.info('Loaded maze from {}.'.format(filename))

    def generate_random_maze(self, size_y, size_x, wall_prob):
        random_array = np.random.rand(size_y, size_x)
        rounder = np.vectorize(lambda t: 1 if t > 1 - wall_prob else 0)
        self.maze_array = rounder(random_array)
        self.dimensions = self.maze_array.shape
        _logger.info('No .xlsx maze file specified - generating a random 10x10 maze with wall_prob=0.5.')

    def is_accessible(self, label):
        return False if self.maze_array[label] == 0 else True

    def create_node(self, label, parent):
        return Square(label, self.find_neighbours(label), parent=parent, accessible=self.is_accessible(label))

    def set_start(self, label):
        if self._check_label_setting(label): self.start = label

    def set_end(self, label):
        if self._check_label_setting(label): self.end = label

    def check_graph(self):
        if self.maze_array.size == 0: raise Exception('No maze geometry specified.')
        if not self.start and not self.end: raise Exception('No start or end labels specified.')
        if not self.start: raise Exception('No start label specified.')
        if not self.end: raise Exception('No end label specified.')
        if self.start == self.end: raise Exception('The start and end labels are identical.')
        return True

    def check_label_on_grid(self, label):
        if not (0 <= label[0] < self.dimensions[0]) or not (0 <= label[1] < self.dimensions[1]): return False
        return True

    def check_label_accessible(self, label):
        if self.maze_array[label] == 0: return False
        return True

    def solve(self, save_history=False):
        if self.check_graph():
            self.solution = a_star.run_a_star(self, self.heuristic_type, save_history)
            if isinstance(self.solution, int):
                _logger.info('Unable to solve the network after {} iterations.'.format(self.solution))
            else:
                self.maze_array_solved = copy(self.maze_array)
                for i, step in enumerate(self.solution[1:-1]): self.maze_array_solved[step] = -i-1
                _logger.info('Successfully solved the GridMaze with {} steps.'.format(len(self.solution)-1))

    def find_neighbours(self, label):
        neighbour_labels = [(label[0] + d[0], label[1] + d[1]) for d in self.straight_coords_deltas
                            if self.check_label_on_grid((label[0] + d[0], label[1] + d[1])) and self.check_label_accessible(label)]
        if self._diagonality:
            neighbour_labels = neighbour_labels + [(label[0] + d[0], label[1] + d[1]) for d in self.diagonal_coord_deltas
                                                   if self.check_label_on_grid((label[0] + d[0], label[1] + d[1])) and self.check_label_accessible(label)]
        return {neighbour_label: self.maze_array[label] for neighbour_label in neighbour_labels}

    def _check_label_setting(self, label):
        tuple_error_msg = 'Coordinates must be a tuple of length 2.'
        if not isinstance(label, tuple): raise Exception(tuple_error_msg)
        if len(label) != 2: raise Exception(tuple_error_msg)
        if not self.check_label_on_grid(label): raise Exception('Label {} is not on the grid (dimensions {}).'.format(label, self.dimensions))
        if not self.is_accessible(label) or label in [self.start, self.end]:
            raise Exception('Cannot place a start or end on top of a wall or an existing start or end.')
        return True


# TODO: Hexagons
# https://www.redblobgames.com/grids/hexagons/

# class Hex(Node):
#
#     hex_type = '' # flat or pointy
#
#     def check_label(self, label):
#
#     def get_cost_to_leave(self, neighbour):
#
#     def get_heuristic_dist(self, other, heuristic_type):
#
#
# class HexGrid(Graph):
#
#     def load_graph(self, filename):
#
#
#     def find_neighbours(self, label):
#
#
#     def is_accessible(self, label):
#
#
#     def create_node(self, label, parent=None):
#
#
#     def solve(self, save_history=False):
#
#
#     def set_start(self, label):
#
#
#     def set_end(self, label):
#
#
#     def check_graph(self):
#
#
#     def _check_label_setting(self):



if __name__ == '__main__':

    # Prepare and solve a maze geometry
    maze = SquareGrid('manhattan', 'walls1.xlsx', diagonality=False)
    maze.set_start((2, 0))
    maze.set_end((8, 9))
    check_maze = maze.check_graph()
    maze.solve(save_history=True)

    # TODO: unit tests
    # # Test failure of start and end placement on wall
    # maze = SquareGrid()
    # maze.load_graph('spiral.xlsx')
    # maze.set_start((4, 1))
    # maze.set_end((5, 8))
    # check_graph = maze.check_graph()

    # TODO: unit tests
    # # Test failure of start placement on existing end
    # maze = SquareGrid()
    # maze.load_graph('spiral.xlsx')
    # maze.set_end((5, 9))
    # maze.set_start((5, 9))
    # check_maze = maze.check_maze()

    # Prepare and solve a network
    net = Network('net1.xlsx')
    net.set_start('A')
    net.set_end('F')
    check_graph = net.check_graph()
    net.solve()

    # TODO: unit tests
    # Test failure of loading network with duplicates
    # netwithduplicate = Network('netwithduplicate.xlsx')

    # Test pathfinding with disjoint network
    netdisjoint = Network('netdisjoint.xlsx')
    netdisjoint.set_start('J')
    netdisjoint.set_end('N')
    check_graph = netdisjoint.check_graph()
    netdisjoint.solve()