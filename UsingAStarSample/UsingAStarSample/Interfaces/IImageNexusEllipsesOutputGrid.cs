// *****************************************************
// Using AStar Sample, created in C#
// By Ben Scharbach
// Image-Nexus, LLC. (4/16/2012)
// *****************************************************
namespace UsingAStarSample.Interfaces
{
    public interface IImageNexusEllipsesOutputGrid
    {
        /// <summary>
        /// Sets the start location for the search.
        /// </summary>
        void SetStart(int row, int column);

        /// <summary>
        /// Sets the goal location for the search.
        /// </summary>
        void SetGoal(int row, int column);

        /// <summary>
        /// Gets the final solution from the AStar engine, and populates the grid with the solution.
        /// </summary>
        void GetSolution();

        /// <summary>
        /// Clears the prior solution from the grid.
        /// </summary>
        void ClearSolution();

        /// <summary>
        /// Clears the OutputGrid of all set nodes.
        /// </summary>
        void ClearOutputGrid();
    }
}