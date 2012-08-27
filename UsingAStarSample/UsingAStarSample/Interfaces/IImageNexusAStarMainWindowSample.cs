// *****************************************************
// Using AStar Sample, created in C#
// By Ben Scharbach
// Image-Nexus, LLC. (4/16/2012)
// *****************************************************
using System.Windows;
using UsingAStarSample.Enums;

namespace UsingAStarSample.Interfaces
{
    public interface IImageNexusAStarMainWindowSample
    {
        /// <summary>
        /// Gets an instance of <see cref="IImageNexusEllipsesOutputGrid"/>.
        /// </summary>
        IImageNexusEllipsesOutputGrid EllipsesOutputGrid { get; }

        /// <summary>
        /// Gets an instance of <see cref="IImageNexusAStarItem"/>
        /// </summary>
        IImageNexusAStarItem AStarItem { get; }

        /// <summary>
        /// Sets message to window.
        /// </summary>
        /// <param name="message">Message to set</param>
        void SetMessage(string message);

        /// <summary>
        /// Adds a <see cref="UIElement"/> to the current OutputGrid.
        /// </summary>
        /// <param name="element">Instance of <see cref="UIElement"/></param>
        void AddChildrenToOutputGrid(UIElement element);

        /// <summary>
        /// Gets the current 'IsChecked' property for the given <see cref="ActionButton"/>.
        /// </summary>
        /// <param name="actionButton">The <see cref="ActionButton"/> to get the 'IsChecked' property.</param>
        /// <returns>true/false</returns>
        bool IsToggleButtonChecked(ActionButton actionButton);
    }
}