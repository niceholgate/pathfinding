import numpy as np
from datetime import datetime
import pickle
from pathlib import Path



import logging
# nm.logging_setup(Path.cwd(), date.today())
_logger = logging.getLogger('pathfinding_logger')

# from geometry import GridCell, GridMaze
from nicpy import nic_misc



def reconstruct_path(current_node):

    path, current = [], current_node
    while current is not None:
        path.append(current.label)
        current = current.parent
    return path[::-1]


def run_a_star(graph, heuristic_type, save_history = False):

    # If saving of algo history requested, create a timestamped directory
    if save_history:
        save_directory = Path.cwd()/'saved_histories'/datetime.now().strftime('%Y-%m-%d_%H_%M_%S')
        nic_misc.mkdir_if_DNE(save_directory)
        _logger.info('Saving algo history to: {}'.format(str(save_directory)))

    # Create start and end nodes
    start_node, end_node = graph.create_node(graph.start, None), graph.create_node(graph.end, None)
    if not start_node.accessible or not end_node.accessible:
        raise Exception('Start and end nodes must both be accessible.')

    # Initialize open and closed lists
    open_list, closed_list = nic_misc.PriorityQueue(), []

    # Add the start node to the open_list
    open_list.put(start_node, 0)

    # Stopping condition
    iterations, max_iterations = 0, 10**6

    current_node = start_node
    while not open_list.empty():
        # Save the current state if requested
        if save_history: _save_history(save_directory, iterations, graph, current_node, open_list, closed_list)

        iterations += 1
        # _logger.warning('{}% max iterations')

        if iterations > max_iterations:
            _logger.error('Exceeded max_iterations ({}), returning path so far.'.format(max_iterations))
            return reconstruct_path(current_node)

        # Get the current node
        current_node = open_list.get()
        closed_list.append(current_node)

        # Found the goal
        if current_node == end_node:
            if save_history: _save_history(save_directory, iterations, graph, current_node, open_list, closed_list)
            return reconstruct_path(current_node)

        # Get accessible, neighbouring children nodes
        children_labels = graph.find_neighbours(current_node.label)
        children = [graph.create_node(label, current_node) for label in children_labels if graph.is_accessible(label)]

        for child in children:
            # Skip child if in closed list
            if child in closed_list: continue

            # Find child's f, g, and h values
            child.g = current_node.g + current_node.get_cost_to_leave(child)
            child.h = child.get_heuristic_dist(end_node, heuristic_type)
            child.f = child.g + child.h

            # Skip child if already in open list with a better g score, otherwise add the child to the open list
            for priority_and_node in open_list.elements:
                if priority_and_node[1] == child and child.g > priority_and_node[1].g: continue
            # if len([open_node for open_node in open_list.elements if child.coords == open_node[1].coords and child.g > open_node[1].g]) > 0: continue
            open_list.put(child, child.f)

    return iterations


def _save_history(save_directory, iterations, graph, current_node, open_list, closed_list):
    if not (save_directory / 'graph.dat').exists(): pickle.dump(graph, open(str(save_directory / 'graph.dat'), 'wb'))  # Just save maze once
    pickle.dump(current_node, open(str(save_directory / '{}_current_node.dat'.format(iterations)), 'wb'))
    pickle.dump(open_list, open(str(save_directory / '{}_open_list.dat'.format(iterations)), 'wb'))
    pickle.dump(closed_list, open(str(save_directory / '{}_closed_list.dat'.format(iterations)), 'wb'))

# if __name__ == '__main__':
#     # Prepare a valid maze geometry
#     maze = Maze()
#     maze.load_excel_maze('spiral_hole3.xlsx')
#     maze.setstart((2, 0))
#     maze.setend((5, 9))
#     check_maze = maze.check_maze()
#
#     path = run_a_star(maze)