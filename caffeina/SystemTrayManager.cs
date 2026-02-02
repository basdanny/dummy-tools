using System.Reflection;

namespace Caffeina
{
    /// <summary>
    /// Manages the system tray icon and context menu for the application
    /// </summary>
    public class SystemTrayManager : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenu;
        private readonly Action _onExitRequested;
        private bool _disposed = false;

        public SystemTrayManager(Action onExitRequested)
        {
            _onExitRequested = onExitRequested ?? throw new ArgumentNullException(nameof(onExitRequested));
            
            // Initialize Windows Forms application if not already done
            if (!Application.MessageLoop)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }
            
            InitializeTrayIcon();
        }

        /// <summary>
        /// Initializes the system tray icon and context menu
        /// </summary>
        private void InitializeTrayIcon()
        {
            try
            {
                // Create context menu
                _contextMenu = new ContextMenuStrip();
                
                var statusItem = new ToolStripMenuItem("Caffeina - Keeping system awake")
                {
                    Enabled = false,
                    Font = new Font(_contextMenu.Font, FontStyle.Bold)
                };
                
                var separatorItem = new ToolStripSeparator();
                
                var exitItem = new ToolStripMenuItem("Exit Caffeina")
                {
                    Image = SystemIcons.Application.ToBitmap()
                };
                exitItem.Click += (sender, e) => _onExitRequested();

                _contextMenu.Items.AddRange(new ToolStripItem[] { statusItem, separatorItem, exitItem });

                // Create notify icon
                _notifyIcon = new NotifyIcon()
                {
                    Icon = CreateCoffeeMugIcon(),
                    Text = "Caffeina - Preventing system sleep",
                    Visible = true,
                    ContextMenuStrip = _contextMenu
                };

                // Double-click to show status
                _notifyIcon.DoubleClick += (sender, e) => ShowStatusBalloon();

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] System tray icon created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Warning: Failed to create system tray icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a coffee mug icon for the system tray from embedded resource
        /// </summary>
        /// <returns>Icon representing a coffee mug</returns>
        private Icon CreateCoffeeMugIcon()
        {            
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Caffeina.resources.coffee.ico";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return new Icon(stream);
                }
                else
                {
                    // Fallback to a default system icon if resource not found
                    return SystemIcons.Application;
                }
            }

        }

        /// <summary>
        /// Shows a status balloon notification
        /// </summary>
        private void ShowStatusBalloon()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.ShowBalloonTip(3000, 
                    "Caffeina Status", 
                    "System sleep prevention is active", 
                    ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// Updates the tooltip text with current status
        /// </summary>
        /// <param name="uptime">Current uptime of the application</param>
        public void UpdateStatus(TimeSpan uptime)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = $"Caffeina - Active for {uptime.Days}d {uptime.Hours:D2}h {uptime.Minutes:D2}m";
            }
        }

        /// <summary>
        /// Shows a notification balloon
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="icon">Notification icon type</param>
        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon?.ShowBalloonTip(5000, title, message, icon);
        }

        /// <summary>
        /// Removes the system tray icon
        /// </summary>
        public void Hide()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] System tray icon hidden");
            }
        }

        /// <summary>
        /// Disposes of system tray resources
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Hide();
                _notifyIcon?.Dispose();
                _contextMenu?.Dispose();
                _disposed = true;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] System tray resources disposed");
            }
        }
    }
}
