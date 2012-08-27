// *****************************************************
// Using AStar Sample, created in C#
// By Ben Scharbach
// Image-Nexus, LLC. (4/16/2012)
// *****************************************************
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Xna.Framework;
using UsingAStarSample.Enums;
using UsingAStarSample.ImageNexus_AStar;
using UsingAStarSample.Interfaces;
using Color = System.Windows.Media.Color;

namespace UsingAStarSample.ImageNexus_OutputGrid
{
    /// <summary>
    /// The <see cref="ImageNexusEllipsesOutputGrid"/> is used to draw ellipses for the AStar results.
    /// </summary>
    public class ImageNexusEllipsesOutputGrid : IImageNexusEllipsesOutputGrid
    {
        #region Fields

        private const int GridRows = 20;
        private const int GridColumns = 20;
        private int _priorStartIndex;
        private int _priorGoalIndex;

        private readonly Color _startColor = Colors.Green;
        private readonly Color _goalColor = Colors.Gold;
        private readonly Color _solutionColor = Colors.Blue;
        private readonly Color _blockedColor = Colors.Red;
        private readonly Color _costColor = Colors.Maroon;
        private readonly Color _emptyColor = Colors.White;
        private readonly Color _ellipseButtonColor = Colors.White;
        private readonly Color _ellipseStrokeColor = Colors.MidnightBlue;

        // Stores refs to the UI Ellipses
        private Dictionary<int, Ellipse> _ellipses;

        // Store prior Start UI Ellipse
        private Ellipse _priorStartEllipse;

        // Store prior Goal UI Ellipse
        private Ellipse _priorGoalEllipse;

        // Ref to the MainWindow parent
        private readonly IImageNexusAStarMainWindowSample _mainWindowParent;

        // Stores the prior solution.
        private readonly Queue<Vector3> _priorSolutionFinal;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainWindow">Instance of the <see cref="IImageNexusAStarMainWindowSample"/></param>
        public ImageNexusEllipsesOutputGrid(IImageNexusAStarMainWindowSample mainWindow)
        {
            if (mainWindow == null)
            {
                throw new ArgumentNullException("mainWindow");
            }

            _mainWindowParent = mainWindow;
            _priorSolutionFinal = new Queue<Vector3>();

            // Populate the AStarOutputGrid with ellipes
            PopulateAStarOutputGrid();

            SetDefaultLocations();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the start location for the search.
        /// </summary>
        public void SetStart(int row, int column)
        {
            var index = row + column * GridColumns;
            _priorStartIndex = index;

            // check if setting to same as goal index
            if (index == _priorGoalIndex)
            {
                _mainWindowParent.SetMessage("You cannot set the start and goal to the same index location!");
                return;
            }
           
            // Update prior Ellipse
            if (_priorStartEllipse != null)
            {
                _priorStartEllipse.Fill = new SolidColorBrush(_emptyColor);
            }

            // Update new Ellipse
            _priorStartEllipse = UpdateEllipseColor(index, _startColor);
           
            // null string message
            _mainWindowParent.SetMessage(string.Empty);
        }

        /// <summary>
        /// Sets the goal location for the search.
        /// </summary>
        public void SetGoal(int row, int column)
        {
            var index = row + column * GridColumns;
            _priorGoalIndex = index;

            // check if setting to same as goal index
            if (index == _priorStartIndex)
            {
                _mainWindowParent.SetMessage("You cannot set the start and goal to the same index location!");
                return;
            }

            // Update prior Ellipse
            if (_priorGoalEllipse != null)
            {
                _priorGoalEllipse.Fill = new SolidColorBrush(_emptyColor);
            }

            // Update new Ellipse
            Ellipse ellipseToUpdate;
            if (_ellipses.TryGetValue(index, out ellipseToUpdate))
            {
                if (ellipseToUpdate != null)
                {
                    ellipseToUpdate.Fill = new SolidColorBrush(_goalColor);
                    _priorGoalEllipse = ellipseToUpdate;
                }
            }

            // null string message
            _mainWindowParent.SetMessage(string.Empty);
        }

        /// <summary>
        /// Gets the final solution from the AStar engine, and populates the grid with the solution.
        /// </summary>
        public void GetSolution()
        {
            // Iterate solution queue and populate grid
            var solutionQueue = _mainWindowParent.AStarItem.SolutionFinal;

            if (solutionQueue == null)
            {
                return;
            }

            while (!solutionQueue.IsEmpty)
            {
                // get node, if available
                Vector3 indexWithStride;
                if (!solutionQueue.TryDequeue(out indexWithStride)) continue;

                // convert to index value by removing the NodeStride.
                var row = (indexWithStride.X > 0) ? (int)indexWithStride.X / ImageNexusAStarItem.NodeStride : (int)indexWithStride.X;
                var column = (indexWithStride.Z > 0) ? (int)indexWithStride.Z / ImageNexusAStarItem.NodeStride : (int)indexWithStride.Z;
                var index = row + column * GridColumns;

                // skip index if matches start of goal locations
                if (index == _priorStartIndex || index == _priorGoalIndex)
                {
                    continue;
                }

                // Update Ellipse color
                UpdateEllipseColor(index, _solutionColor);

                // Add index to prior solution queue.
                _priorSolutionFinal.Enqueue(indexWithStride);
            }
        }

        /// <summary>
        /// Clears the prior solution from the grid.
        /// </summary>
        public void ClearSolution()
        {
            // Iterate solution queue and populate grid
            var solutionQueue = _priorSolutionFinal;

            if (solutionQueue == null)
            {
                return;
            }

            while (solutionQueue.Count != 0)
            {
                // get vector
                var indexWithStride = _priorSolutionFinal.Dequeue();

                // convert to index value by removing the NodeStride.
                var row = (indexWithStride.X > 0) ? (int)indexWithStride.X / ImageNexusAStarItem.NodeStride : (int)indexWithStride.X;
                var column = (indexWithStride.Z > 0) ? (int)indexWithStride.Z / ImageNexusAStarItem.NodeStride : (int)indexWithStride.Z;
                var index = row + column * GridColumns;

                // skip blocked nodes
                if (_mainWindowParent.AStarItem.IsBlocked(row, column))
                {
                    continue;
                }

                // Update Ellipse color
                UpdateEllipseColor(index, _emptyColor);
            }
        }

        /// <summary>
        /// Clears the OutputGrid of all set nodes.
        /// </summary>
        public void ClearOutputGrid()
        {
            ClearSolution();
            ClearAllBlocks();
            SetDefaultLocations();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Clears all set blocked locations.
        /// </summary>
        private void ClearAllBlocks()
        {
            // iterate rows
            for (int i = 0; i < GridRows; i++)
            {
                // iterate columns
                for (int j = 0; j < GridColumns; j++)
                {
                    var index = i + j * GridColumns;

                    Ellipse ellipse;
                    if (_ellipses.TryGetValue(index, out ellipse))
                    {
                        SetBlock(ellipse, true);
                    }
                }
            }
        }

        /// <summary>
        /// Populates the WPF Grid with ellipes.
        /// </summary>
        private void PopulateAStarOutputGrid()
        {
            // Create dictionary to hold ref to ellipses
            _ellipses = new Dictionary<int, Ellipse>(GridRows * GridColumns);
             
            // iterate rows
            for (int i = 0; i < GridRows; i++)
            {
                // iterate columns
                for (int j = 0; j < GridColumns; j++)
                {
                    // Create new Ellipse UI control
                    var ellipse = new Ellipse
                                    {
                                        Margin = new Thickness(0, 0, 0, 0),
                                        Height = 7,
                                        Width = 7,
                                        Stroke = new SolidColorBrush(_ellipseStrokeColor),
                                        Name = string.Format("ellipse_{0}_{1}", i, j),
                                        Tag = new ImageNexusEllipsesTagData
                                                  {
                                                      IndexRow = i,
                                                      IndexColumn = j,
                                                  }
                                    };

                    // Add Ellipse UI control to the dictionary
                    var index = i + j * GridColumns;
                    _ellipses.Add(index, ellipse);

                    // Create new button as ellipse template
                    var buttonEllipse = new Button
                                            {
                                                Height = 15,
                                                Width = 20,
                                                Name = string.Format("buttonEllipse{0}_{1}", i, j),
                                                Content = ellipse,
                                                Background = new SolidColorBrush(_ellipseButtonColor)
                                            };
                    buttonEllipse.Click += buttonEllipse_Click;

                    // Add Ellipse UI control to the grid
                    Grid.SetRow(buttonEllipse, i);
                    Grid.SetColumn(buttonEllipse, j);

                    // Add to gridviews children collection
                    _mainWindowParent.AddChildrenToOutputGrid(buttonEllipse);
                }
            }
        }
        
        /// <summary>
        /// Occurs when the user clicks on some Ellipse to apply blockage.
        /// </summary>
        private void buttonEllipse_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var buttonControl = (Button) sender;
            if (buttonControl == null) return;

            // retrieve the ellipse content
            var ellipseInstance = (Ellipse) buttonControl.Content;

            // Check if 'Set Start' is toggled.
            if (_mainWindowParent.IsToggleButtonChecked(ActionButton.StartToggleButton))
            {
                SetStart(ellipseInstance);  
            }

            // Check if 'Set Goal' is toggled.
            if (_mainWindowParent.IsToggleButtonChecked(ActionButton.GoalToggleButton))
            {
                SetGoal(ellipseInstance);  
            }

            // Check if 'Set Blocks' is toggled.
            if(_mainWindowParent.IsToggleButtonChecked(ActionButton.BlockedToggleButton))
            {
                SetBlock(ellipseInstance);  
            }
        }

        /// <summary>
        /// Used to set an ellipse as a start position.
        /// </summary>
        /// <param name="ellipseInstance">Instance of Ellipse</param>
        private void SetStart(Ellipse ellipseInstance)
        {
            // parse out the index values from the 'Tag'
            var ellipsesTagData = (ImageNexusEllipsesTagData)ellipseInstance.Tag;

            // check if index location is already blocked
            if (_mainWindowParent.AStarItem.IsBlocked(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn))
            {
                _mainWindowParent.SetMessage("You cannot set the start to a blocked location!");
                return;
            }

            // Set into AStarItem's graph
            _mainWindowParent.AStarItem.SetStart(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn);

            // Set Start position in Ellipse's output grid
            SetStart(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn);
        }

        /// <summary>
        /// Used to set an ellipse as a goal position.
        /// </summary>
        /// <param name="ellipseInstance">Instance of Ellipse</param>
        private void SetGoal(Ellipse ellipseInstance)
        {
            // parse out the index values from the 'Tag'
            var ellipsesTagData = (ImageNexusEllipsesTagData)ellipseInstance.Tag;

            // check if index location is already blocked
            if (_mainWindowParent.AStarItem.IsBlocked(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn))
            {
                _mainWindowParent.SetMessage("You cannot set the goal to a blocked location!");
                return;
            }

            // Set into AStarItem's graph
            _mainWindowParent.AStarItem.SetGoal(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn);

            // Set Start position in Ellipse's output grid
            SetGoal(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn);
        }

        /// <summary>
        /// Used to set an ellipse as a block cost.
        /// </summary>
        /// <param name="ellipseInstance">Instance of Ellipse</param>
        /// <param name="clearAction">(Optional) Set when using the 'SetBlock' to clear nodes.</param>
        private void SetBlock(Ellipse ellipseInstance, bool clearAction = false)
        {
            // parse out the index values from the 'Tag'
            var ellipsesTagData = (ImageNexusEllipsesTagData) ellipseInstance.Tag;

            // check if index location is already blocked
            if (_mainWindowParent.AStarItem.IsBlocked(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn))
            {
                _mainWindowParent.AStarItem.RemoveBlock(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn);
                ellipseInstance.Fill = new SolidColorBrush(Colors.White);
                return;
            }

            // check if 'ClearAction'
            if (clearAction)
            {
                return;
            }

            // set block cost to this instance in the AStarItem
            _mainWindowParent.AStarItem.SetAsBlocked(ellipsesTagData.IndexRow, ellipsesTagData.IndexColumn);
            ellipseInstance.Fill = new SolidColorBrush((_mainWindowParent.AStarItem.BlockCost == -1) ? _blockedColor : _costColor);
        }

        /// <summary>
        /// Updates a given Ellipse by the index location.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="colorToUse"> </param>
        private Ellipse UpdateEllipseColor(int index, Color colorToUse)
        {
            Ellipse ellipseToUpdate;
            if (!_ellipses.TryGetValue(index, out ellipseToUpdate)) return null;
            if (ellipseToUpdate == null) return null;

            ellipseToUpdate.Fill = new SolidColorBrush(colorToUse);

            return ellipseToUpdate;
        }

        /// <summary>
        /// Sets the Start and End default locations.
        /// </summary>
        private void SetDefaultLocations()
        {
            // Show default locations on map
            SetStart(0, 0);
            SetGoal(10, 10);

            // Set into AStar Component
            _mainWindowParent.AStarItem.SetStart(0, 0);
            _mainWindowParent.AStarItem.SetGoal(10, 10);
        }

        #endregion
    }
}