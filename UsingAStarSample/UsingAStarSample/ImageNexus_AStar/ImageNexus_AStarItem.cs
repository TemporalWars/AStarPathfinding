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
using UsingAStarSample.ImageNexus_LateBinder;
using UsingAStarSample.Interfaces;

namespace UsingAStarSample.ImageNexus_AStar
{
    /// <summary>
    /// The <see cref="ImageNexusAStarItem"/> class inherits from the IAStarItem and provides AStar functionality.
    /// </summary>
    public class ImageNexusAStarItem : IAStarItem, IImageNexusAStarItem
    {
        #region Fields

        private float _blockCost = 500;
        private readonly IAStarManager _aStarManager; // LateBind component
        private const int _nodeStride = 90;
        private const int _nodeArraySize = 20;

        #endregion

        #region Events

        /// <summary>
        /// Triggers when a valid solution is found.
        /// </summary>
        public event EventHandler SolutionCompleted;

        /// <summary>
        /// Triggers when the no solution was found.
        /// </summary>
        public event EventHandler SolutionFailed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current NodeStride, which is the distance between each node.
        /// </summary>
        public static int NodeStride
        {
            get { return _nodeStride; }
        }

        /// <summary>
        /// Gets the current A* path node size, or number of nodes in
        /// the given graph; for example, 10 is 10x10.
        /// </summary>
        public static int NodeArraySize
        {
            get { return _nodeArraySize; }
        }

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
        public float BlockCost
        {
            get { return _blockCost; }
            set { _blockCost = value; }
        }

        ///<summary>
        /// Interface reference for <see cref="IAStarGraph"/>.
        ///</summary>
        public static IAStarGraph AStarGraph { get; private set; }

        /// <summary>
        /// Vector3 goal position for the given unit.
        ///</summary>
        public Vector3 GoalPosition { get; set; }
        

        /// <summary>
        /// Current A-Star node position in game world.
        ///</summary>
        public Vector3 PathNodePosition { get; set; }
        

        /// <summary>
        /// Stores the final validated A-Star solution, returned from
        /// the A-Star engine.
        ///</summary>
        /// <remarks>This Property is Thread-Safe.</remarks>
        public LocklessQueue<Vector3> SolutionFinal { get; set; }
        

        /// <summary>
        /// Able to pass over blocked areas?
        /// </summary>
        public bool CanPassOverBlockedAreas { get; set; }
       

        /// <summary>
        ///  IgnoreOccupiedBy for PathFinding? - If a unit has this flag set On,
        ///  then the A* will ignore the 'occupiedBy' Status when creating a valid path.
        ///  As the unit moves along the path, any blocking units will move out of the way! 
        /// </summary>
        public IgnoreOccupiedBy IgnoreOccupiedByFlag { get; set; }
        

        /// <summary>
        /// Enum to signify a Ground SceneItemOwner or Air SceneItemOwner, for
        /// the given SceneItem.
        /// </summary>
        public PathNodeType UsePathNodeType { get; private set; }
       

        /// <summary>
        /// During A-Star solutions, this is checked to determine if the 
        /// original 'Start' and 'End' nodes should be used; otherwise, if set 'TRUE', 
        /// the closest node to the 'End' node, from the 'Start' node, will be used.
        ///</summary>
        public AdjToClosestNode SetAdjToClosestNode { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageNexusAStarItem()
        {
            SolutionFinal = new LocklessQueue<Vector3>();

            // Add AStar Component Class; 6/16/2009: Currently, method 'InitAStarEngines', called in LoadHeightData of 'TerrainData' class.
            object lateBindAssembly;
            if (!LateBinder.LateBindAssembly("AStarComponentLibrary.dll", "AStarManager", out lateBindAssembly))
            {
                throw new InvalidOperationException("The 'AStarComponentLibrary.dll' failed to latebind, which is a requirement for this sample!");
            }


            // cast to proper interface
            _aStarManager = (IAStarManager) lateBindAssembly;
            _aStarManager.CommonInitilization(null);
            AStarGraph = _aStarManager.IAStarGraph;

            // Distance between nodes
            AStarGraph.NodeStride = _nodeStride;

            // Stores the A* path node size, or number of nodes in
            // the given graph; for example, 10 is 10x10.
            AStarGraph.NodeArraySize = _nodeArraySize;

            // Create Graph
            AStarGraph.CreatePathfindingGraph();

            // Init the AStar Engine threads
            _aStarManager.InitAStarEngines(AStarGraph.NodeArraySize);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends a solution request to the AStarManager component.
        /// </summary>
        public void FindSolution()
        {
            if (_aStarManager == null)
            {
                throw new InvalidOperationException("The LateBind assembly 'AStarManager' is null!");
            }

            // Clear solution queue
            ClearSolutionFinal(this);

            // Start A* Search
            SetAdjToClosestNode = AdjToClosestNode.On;
            _aStarManager.FindPath_Init(this);

            // Note: Since this Sample is not using the XNA 'Game' component, consquently, the
            //       'Update' call is not done automatically.  Therfore, we will call this 'Update' directly.
            ((GameComponent)_aStarManager).Update(null);
        }

        /// <summary>
        /// Sets the start location for the search.
        /// </summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        public void SetStart(int row, int column)
        {
            PathNodePosition = new Vector3
                                   {
                                       X = row,
                                       Y = 0,
                                       Z = column
                                   };

            var index = new Point { X = row, Y = column };
            AStarGraph.SetOccupiedByToIndex(ref index, NodeScale.AStarPathScale, UsePathNodeType, null);
        }

        /// <summary>
        /// Sets the goal location for the search.
        /// </summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        public void SetGoal(int row, int column)
        {
            // since AStarGraph expects the index to include the stride, let's multiply by NodeStride.
            GoalPosition = new Vector3(row * AStarGraph.NodeStride, 0, column * AStarGraph.NodeStride);
        }

        /// <summary>
        /// Is the current position already blocked?
        /// </summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        /// <returns>true/false</returns>
        public bool IsBlocked(int row, int column)
        {
            float nodeCost;
            AStarGraph.GetNodeCost(row, column, out nodeCost);

            return Math.Abs(nodeCost - BlockCost) < float.Epsilon;
        }

        ///<summary>
        /// Sets a Cost Value in the A* Algorithm at the given MapNode.
        ///</summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        public void SetAsBlocked(int row, int column)
        {
            // since AStarGraph expects the index to include the stride, let's multiply by NodeStride.
            AStarGraph.SetCostToPos(row * AStarGraph.NodeStride, column * AStarGraph.NodeStride, BlockCost, 1);
        }

        ///<summary>
        /// Remove Cost Value from the A* Graph at the current Position.
        ///</summary>
        ///<param name="column">Column's index value</param>
        ///<param name="row">Row's index value</param>
        public void RemoveBlock(int row, int column)
        {
            // since AStarGraph expects the index to include the stride, let's multiply by NodeStride.
            AStarGraph.RemoveCostAtPos(row * AStarGraph.NodeStride, column * AStarGraph.NodeStride, 1);
        }

        /// <summary>
        /// When using the cost of -1 for blocked nodes, the node is removed from the search area within AStar engine.
        /// Therefore, this will force the graph to recreate all node connections.
        /// </summary>
        public void ReInitializeSearchGrid()
        {
            if (_aStarManager != null)
            {
                _aStarManager.ReInitAStarArrays(_nodeArraySize);
            }
        }

        /// <summary>
        /// Search Complete method, called by the 'CycleOnce' AStar engine, when search complete.
        /// </summary>
        /// <param name="astarItem"><see cref="IAStarItem"/> instance</param>
        /// <param name="solutionFound">Solution found?</param>
        public void AStarInstanceSearchComplete(IAStarItem astarItem, bool solutionFound)
        {
            if (solutionFound)
            {
                if (SolutionCompleted != null)
                {
                    SolutionCompleted(this, EventArgs.Empty);
                }
            }
            else
            {
                if (SolutionFailed != null)
                {
                    SolutionFailed(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method helper, which iterates the <see cref="SolutionFinal"/> queue, calling
        /// the tryDequeue method until all nodes are removed.
        /// </summary>
        /// <param name="astarItem"><see cref="IAStarItem"/> instance</param>
        private static void ClearSolutionFinal(IAStarItem astarItem)
        {
            Vector3 result;
            while (!astarItem.SolutionFinal.IsEmpty)
                astarItem.SolutionFinal.TryDequeue(out result);
        }

        #endregion
    }
}