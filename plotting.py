import pickle
from pathlib import Path

import matplotlib
import matplotlib.pyplot as plt
import matplotlib.patches as ptc

from nicpy import nic_misc, nic_pic
import a_star
from geometry import Square, SquareGrid


def algo_animation(history_directory, fps=2, frame_skip=1):

    matplotlib.use('Agg')   # Gets rid of plot popups, faster to produce frames
    max_iteration = 0       # Find the number of iterations saved
    for file in history_directory.iterdir():
        if file.is_file():
            string_iter_num = str(file.stem).split('_')[0]
            if string_iter_num != 'graph':
                if int(string_iter_num)>max_iteration: max_iteration = int(string_iter_num)

    # Save a frame for each algo iteration
    counter = 0
    frames_directory = history_directory/'frames'
    nic_misc.mkdir_if_DNE(frames_directory)
    its = list(range(0, max_iteration+1, frame_skip))
    if its[-1] != max_iteration: its.append(max_iteration)
    for iteration in its:
        print('Producing image {} of {}'.format(iteration+1, max_iteration+1))
        frame = plot_algo_state(history_directory, iteration)
        frame.savefig(frames_directory/'frame_{}.png'.format(str(counter).zfill(5)), bbox_inches='tight')
        counter += 1
        plt.close(frame)

    nic_pic.frames_to_video(frames_directory, video_name_with_format='video.avi', image_format='png', fps=fps)

def plot_algo_state(history_directory, iteration):
    maze = pickle.load(open(str(history_directory/'graph.dat'.format(iteration)), 'rb'))
    closed_list = pickle.load(open(str(history_directory/'{}_closed_list.dat'.format(iteration)), 'rb'))
    open_list = pickle.load(open(str(history_directory/'{}_open_list.dat'.format(iteration)), 'rb'))
    current_node = pickle.load(open(str(history_directory/'{}_current_node.dat'.format(iteration)), 'rb'))
    current_path = a_star.reconstruct_path(current_node)
    fig = plot_maze(maze, current_path, closed_list, open_list)
    return fig

def plot_maze(maze, current_path=None, closed_list=None, open_list=None):
    maze.check_graph()
    fig, ax = plt.subplots(1, dpi=100)
    plt.gca().invert_yaxis()
    ax.axis('equal')
    ax.set_facecolor([0.5, 0.5, 0.5])
    patch_width = 1

    # Add lines on top layers
    for row in range(maze.dimensions[0]+1):
        ax.plot([0, maze.dimensions[1]], [row, row], 'k', zorder=102, linewidth=3)
    for col in range(maze.dimensions[1]+1):
        ax.plot([col, col], [0, maze.dimensions[0]], 'k', zorder=103, linewidth=3)

    # Add colour patches
    for row in range(maze.dimensions[0]):
        for col in range(maze.dimensions[1]):
            if maze.maze_array[row, col] > 0: ax.add_patch(ptc.Rectangle([col, row], patch_width, patch_width, color = [1, 1, 1]))
            if maze.maze_array[row, col] == 0: ax.add_patch(ptc.Rectangle([col, row], patch_width, patch_width, color=[0.3, 0.2, 0.1], zorder=100))
            elif (row, col) == maze.start: ax.add_patch(ptc.Rectangle([col, row], patch_width, patch_width, color='r', zorder=101))
            elif (row, col) == maze.end: ax.add_patch(ptc.Rectangle([col, row], patch_width, patch_width, color='g'))

    # These elements change each time
    if closed_list:
        for node in closed_list:
            ax.add_patch(ptc.Rectangle((node.label[1], node.label[0]), patch_width, patch_width, color='y'))
    if open_list:
        for pair in open_list.elements:
            node = pair[1]
            ax.add_patch(ptc.Rectangle((node.label[1], node.label[0]), patch_width, patch_width, color='m'))
    if current_path:
        for i, step in enumerate(current_path[1:-1]):
            # ax.add_patch(ptc.Rectangle((step[1], step[0]), 1, 1, color='b'))
            ax.text(step[1]+0.5, step[0]+0.5, 'x', color='b', horizontalalignment='center', verticalalignment='center')

    return fig

if __name__ == '__main__':
    # Plot an algo state
    plot_algo_state(Path(r'E:\dev\python_projects\pathfinding\saved_histories\2020-08-16_15_13_19'), 40)

    # Create an animation
    algo_animation(Path(r'E:\dev\python_projects\pathfinding\saved_histories\2020-08-16_15_13_19'), fps=5, frame_skip = 1)
