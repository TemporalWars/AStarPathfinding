// *****************************************************
// Using AStar Sample, created in C#
// By Ben Scharbach
// Image-Nexus, LLC. (4/16/2012)
// 
// This AStar sample shows how to use the bindable XNA component 'AStarComponent' in a simple
// WPF application.  The sample demonstrates how to wire up the AStarComponent.dll to use the 
// AStarManager and AStarGraph.
//
// The AStarComponent is a powerful Multi-thread engine, designed to take advantage of the CPU's
// multiprocessors and the XBOX's multi-thread capabilities, while maintaining a low garbage 
// collection count.  The engine is designed using 'Time-Slices', allowing multiple AStar pathfinding
// requests to occur at the same time.  
//
// The AStarComponent engine has multiple state settings, like the ability to optimize for redudant straight
// paths, closest start node auto-placement, changable NodeStride distance placement, and updatable cost choice
// algorithms.
//
// However, to keep it simple, this example will focus on the basics of setting a 'Start' and 'Goal' location, and
// calling the proper 'Find' path method calls.  The ability to set block paths is shown, as well as setting the
// blocked 'Cost' value to -1, which creates impassiable walls.
// *****************************************************
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UsingAStarSample.Enums;
using UsingAStarSample.ImageNexus_AStar;
using UsingAStarSample.ImageNexus_OutputGrid;
using UsingAStarSample.Interfaces;

namespace UsingAStarSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IImageNexusAStarMainWindowSample
    {
        #region Fields

        private readonly IImageNexusEllipsesOutputGrid _ellipsesOutputGrid;
        private readonly IImageNexusAStarItem _aStarItem;

        #endregion

        #region Properties

        /// <summary>
        /// Gets an instance of <see cref="IImageNexusEllipsesOutputGrid"/>.
        /// </summary>
        public IImageNexusEllipsesOutputGrid EllipsesOutputGrid
        {
            get { return _ellipsesOutputGrid; }
        }

        /// <summary>
        /// Gets an instance of <see cref="IImageNexusAStarItem"/>
        /// </summary>
        public IImageNexusAStarItem AStarItem
        {
            get { return _aStarItem; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor which initializes the ellipses
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Create instance of the AStarItem class
            _aStarItem = new ImageNexusAStarItem();

            // Create instance of the EllipsesOutputGrid class
            _ellipsesOutputGrid = new ImageNexusEllipsesOutputGrid(this);

            // Wire up event handlers for the AStar engine
            _aStarItem.SolutionCompleted += new EventHandler(AStarItem_SolutionCompleted);
            _aStarItem.SolutionFailed += new EventHandler(AStarItem_SolutionFailed);

            // Set Instructions into header block
            txtHeaderBlock.Text = "This example shows how to wire up the AStar engine component," + 
                " and use the powerful search capabilities.  Simple set your 'Start' and 'Goal' "+
                "positions on the graph below, set your blocked areas and then click 'Find Solution'.";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets message to window.
        /// </summary>
        /// <param name="message">Message to set</param>
        public void SetMessage(string message)
        {
            txtMessages.Text = message;
        }

        /// <summary>
        /// Adds a <see cref="UIElement"/> to the current OutputGrid.
        /// </summary>
        /// <param name="element">Instance of <see cref="UIElement"/></param>
        public void AddChildrenToOutputGrid(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (astarOutputGrid != null)
            {
                astarOutputGrid.Children.Add(element);
            }
        }

        /// <summary>
        /// Gets the current 'IsChecked' property for the given <see cref="ActionButton"/>.
        /// </summary>
        /// <param name="actionButton">The <see cref="ActionButton"/> to get the 'IsChecked' property.</param>
        /// <returns>true/false</returns>
        public bool IsToggleButtonChecked(ActionButton actionButton)
        {
            switch (actionButton)
            {
                case ActionButton.StartToggleButton:
                    return btnSetStartLocation.IsChecked ?? false;
                case ActionButton.GoalToggleButton:
                    return btnSetEndLocation.IsChecked ?? false;
                case ActionButton.BlockedToggleButton:
                    return btnSetBlocks.IsChecked ?? false;
                default:
                    throw new ArgumentOutOfRangeException("actionButton");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Occurs when the user clicks the StartFind request button
        /// </summary>
        private void btnStartFind_Click(object sender, RoutedEventArgs e)
        {
            // Clear out the OutputGrid's view of any prior solutions
            if (_ellipsesOutputGrid != null)
            {
                _ellipsesOutputGrid.ClearSolution();
            }

            // Start new solution find
            if (_aStarItem != null)
            {
                _aStarItem.FindSolution();
            }
        }

        /// <summary>
        /// Occurs when the user clicks on the 'ReInit' button.
        /// </summary>
        private void btnReInitSearchArea_Click(object sender, RoutedEventArgs e)
        {
            // Rebuild search grid.
            if (_aStarItem != null)
            {
                _aStarItem.ReInitializeSearchGrid();
            }
        }

        /// <summary>
        /// Occurs when the user click the 'Set' block cost button.
        /// </summary>
        private void btnSetBlockCost_Click(object sender, RoutedEventArgs e)
        {
            // get the value contain in the text block
            var blockCost = Convert.ToInt32(txtBlockCost.Text);

            if (blockCost == 0)
            {
                txtMessages.Text = "You must set a cost value which is not zero.";
            }

            if (blockCost > 100)
            {
                txtMessages.Text = "You must set a cost value which is less than 100.";
            }

            if (_aStarItem != null)
            {
                _aStarItem.BlockCost = blockCost;
                txtMessages.Text = string.Empty;
            }
        }

        /// <summary>
        /// Occurs when the user clicks the clear button.
        /// </summary>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (_ellipsesOutputGrid != null)
            {
                _ellipsesOutputGrid.ClearOutputGrid();
            }
        }

        /// <summary>
        /// Occurs when the 'Set Start' is enabled.
        /// </summary>
        private void btnSetStartLocation_Checked(object sender, RoutedEventArgs e)
        {
            // Unset the other two states.
            btnSetEndLocation.IsChecked = false;
            btnSetBlocks.IsChecked = false;
        }

        /// <summary>
        /// Occurs when the 'Set Start' is NOT enabled.
        /// </summary>
        private void btnSetStartLocation_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Occurs when the 'Set Goal' is enabled.
        /// </summary>
        private void btnSetEndLocation_Checked(object sender, RoutedEventArgs e)
        {
            // Unset the other two states.
            btnSetStartLocation.IsChecked = false;
            btnSetBlocks.IsChecked = false;
        }

        /// <summary>
        /// Occurs when the 'Set Goal' is NOT enabled.
        /// </summary>
        private void btnSetEndLocation_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Occurs when the 'Set Blocks' is enabled.
        /// </summary>
        private void btnSetBlocks_Checked(object sender, RoutedEventArgs e)
        {
            // Unset the other two states.
            btnSetEndLocation.IsChecked = false;
            btnSetStartLocation.IsChecked = false;
        }

        /// <summary>
        /// Occurs when the 'Set Blocks' is NOT enabled.
        /// </summary>
        private void btnSetBlocks_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Occurs when the AStar engine completed event is triggered.
        /// </summary>
        private void AStarItem_SolutionCompleted(object sender, EventArgs e)
        {
            // Since this comes from a different calling thread, the 'Dispatcher' needs to be used to send it to the WPF UI thread.
            Dispatcher.Invoke(DispatcherPriority.Send, new Action<TextBox>(delegate(TextBox textBox)
                                                                               {
                                                                                   txtMessages.Text = "AStar engine found a solution.";
                                                                               }), txtMessages);


            // Since this comes from a different calling thread, the 'Dispatcher' needs to be used to send it to the WPF UI thread.
            Dispatcher.Invoke(DispatcherPriority.Send, new Action<ImageNexusEllipsesOutputGrid>(delegate(ImageNexusEllipsesOutputGrid ellipsesOutputGrid)
                                                                               {
                                                                                   // Populate OutputGrid
                                                                                   ellipsesOutputGrid.GetSolution();
                                                                               }), _ellipsesOutputGrid);
            
        }

        /// <summary>
        /// Occurs when the AStar engine failed event is triggered.
        /// </summary>
        private void AStarItem_SolutionFailed(object sender, EventArgs e)
        {
            // Since this comes from a different calling thread, the 'Dispatcher' needs to be used to send it to the WPF UI thread.
            Dispatcher.Invoke(DispatcherPriority.Send, new Action<TextBox>(delegate(TextBox textBox)
                                                                               {
                                                                                   txtMessages.Text = "AStar engine did not find a solution!";
                                                                               }), txtMessages);
        }

        #endregion

      
    }
}
