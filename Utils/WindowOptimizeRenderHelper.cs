using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace vJassMainJBlueprint.Utils
{
    /// <summary>
    /// Provides optimized window rendering support by minimizing layout recalculations during window resizing.
    /// </summary>
    public static class WindowOptimizeRenderHelper
    {
        private static readonly TimeSpan LayoutUpdateDelay = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Optimizes rendering by applying resize optimizations to the specified window and grids.
        /// </summary>
        /// <param name="window">The window to optimize rendering for.</param>
        /// <param name="clientArea">The client area grid of the window.</param>
        /// <param name="footerContainer">The footer container grid of the window.</param>
        public static void OptimizeRender(Window window, Grid clientArea, Grid footerContainer)
        {
            ApplyOptimizedVisualTemplate(window, clientArea, footerContainer);
            ApplyOptimizedLayoutBehavior(window, clientArea, footerContainer);
        }

        private static void ApplyOptimizedVisualTemplate(Window window, Grid clientArea, Grid footerContainer)
        {
            // Set the window background to black to prevent flickering during resize operations.
            window.Background = Brushes.Black;
            // Detach footer container from the visual tree to prevent layout recalculations.
            footerContainer.ClipToBounds = true;
            footerContainer.Width = clientArea.ActualWidth;
            footerContainer.Height = clientArea.ActualHeight;
            footerContainer.HorizontalAlignment = HorizontalAlignment.Left;
            footerContainer.VerticalAlignment = VerticalAlignment.Top;
        }

        private static void ApplyOptimizedLayoutBehavior(Window window, Grid clientArea, Grid footerContainer)
        {
            // Timer to delay the resize operation and reduce frequent layout updates.
            DispatcherTimer resizeTimer = new() { Interval = LayoutUpdateDelay };

            // Event handler for window size changes.
            window.SizeChanged += (sender, e) =>
            {
                // Start or reset the timer when window size changes.
                if (!resizeTimer.IsEnabled)
                {
                    resizeTimer.Start();
                }
            };

            // Event handler for the timer tick to apply the resize optimization.
            resizeTimer.Tick += (sender, e) =>
            {
                // Ensure the UI thread is used to update the layout.
                window.Dispatcher.Invoke(() =>
                {
                    double clientWidth = clientArea.ActualWidth;
                    double clientHeight = clientArea.ActualHeight;

                    // Only update footer size if it differs from the client area size to prevent unnecessary layout recalculations.
                    if (Math.Abs(footerContainer.ActualWidth - clientWidth) > 0.1 ||
                        Math.Abs(footerContainer.ActualHeight - clientHeight) > 0.1)
                    {
                        footerContainer.Width = clientWidth;
                        footerContainer.Height = clientHeight;
                    }
                    else
                    {
                        // Stop the timer when the footer is correctly resized.
                        resizeTimer.Stop();
                    }
                });
            };
        }
    }
}
