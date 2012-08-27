// *****************************************************
// Using AStar Sample, created in C#
// By Ben Scharbach
// Image-Nexus, LLC. (4/16/2012)
// *****************************************************
using System;
using AStarInterfaces.AStarAlgorithm;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using ParallelTasksComponent.LocklessQueue;

namespace UsingAStarSample.Interfaces
{
    public interface IImageNexusAStarItem
    {
        /// <summary>
        /// Triggers when a valid solution is found.
        /// </summary>
        event EventHandler SolutionCompleted;

        /// <summary>
        /// Triggers when the no solution was found.
        /// </summary>
        event EventHandler SolutionFailed;

        /// <summary>
        /// Gets or sets the block cost of a node position.  The higher the cost, the less likely the
        /// search algorithm will use that node for a viable solution.  However, this does not imply the
        /// node is off-limits; use -1 for impassiable walls.
        /// </summary>
        /// <remarks>
        /// Use the cost of '-1' to create impassiable walls. Internally in the AStar engine, this
        /// will remove the node from a search.  Therefore, it is important to click the 'Re-Init'
        /// button when changing the location of impassiable walls, since the node was original removed 
        /// from the search area.
        /// </remarks>
        float BlockCost { get; set; }

        /// <summary>
        /// Vector3 goal position for the given unit.
        ///</summary>
        Vector3 GoalPosition { get; set; }

        /// <summary>
        /// Current A-Star node position in game world.
        ///</summary>
        Vector3 PathNodePosition { get; set; }

        /// <summary>
        /// Stores the final validated A-Star solution, returned from
        /// the A-Star engine.
        ///</summary>
        /// <remarks>This Property is Thread-Safe.</remarks>
        LocklessQueue<Vector3> SolutionFinal { get; set; }

        /// <summary>
        /// Able to pass over blocked areas?
        /// </summary>
        bool CanPassOverBlockedAreas { get; set; }

        /// <summary>
        ///  IgnoreOccupiedBy for PathFinding? - If a unit has this flag set On,
        ///  then the A* will ignore the 'occupiedBy' Status when creating a valid path.
        ///  As the unit moves along the path, any blocking units will move out of the way! 
        /// </summary>
        IgnoreOccupiedBy IgnoreOccupiedByFlag { get; set; }

        /// <summary>
        /// Enum to signify a Ground SceneItemOwner or Air SceneItemOwner, for
        /// the given SceneItem.
        /// </summary>
        PathNodeType UsePathNodeType { get; }

        /// <summary>
        /// During A-Star solutions, this is checked to determine if the 
        /// original 'Start' and 'End' nodes should be used; otherwise, if set 'TRUE', 
        /// the closest node to the 'End' node, from the 'Start' node, will be used.
        ///</summary>
        AdjToClosestNode SetAdjToClosestNode { get; set; }

        /// <summary>
        /// Sends a solution request to the AStarManager component.
        /// </summary>
        void FindSolution();

        /// <summary>
        /// Sets the start location for the search.
        /// </summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        void SetStart(int row, int column);

        /// <summary>
        /// Sets the goal location for the search.
        /// </summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        void SetGoal(int row, int column);

        /// <summary>
        /// Is the current position already blocked?
        /// </summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        /// <returns>true/false</returns>
        bool IsBlocked(int row, int column);

        ///<summary>
        /// Sets a Cost Value in the A* Algorithm at the given MapNode.
        ///</summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        void SetAsBlocked(int row, int column);

        ///<summary>
        /// Remove Cost Value from the A* Graph at the current Position.
        ///</summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        void RemoveBlock(int row, int column);

        /// <summary>
        /// When using the cost of -1 for blocked nodes, the node is removed from the search area within AStar engine.
        /// Therefore, this will force the graph to recreate all node connections.
        /// </summary>
        void ReInitializeSearchGrid();

        /// <summary>
        /// Search Complete method, called by the 'CycleOnce' AStar engine, when search complete.
        /// </summary>
        /// <param name="astarItem"><see cref="IAStarItem"/> instance</param>
        /// <param name="solutionFound">Solution found?</param>
        void AStarInstanceSearchComplete(IAStarItem astarItem, bool solutionFound);
    }
}