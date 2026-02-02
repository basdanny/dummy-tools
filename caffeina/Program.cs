using System.Text;
using System.Windows.Forms;

namespace Caffeina
{
    /// <summary>
    /// Main application entry point and console interface
    /// </summary>
    public class Program
    {
        private static KeepAliveManager? _keepAliveManager;
        private static SystemTrayManager? _trayManager;
        private static readonly object _lockObject = new();
        private static bool _isRunning = false;
        private static bool _exitRequested = false;
        private static DateTime _startTime;

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Initialize Windows Forms before creating any Windows Forms objects
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Parse command line arguments
                var config = ParseArguments(args);
                
                // Display startup banner
                DisplayBanner();
                
                // Initialize components
                InitializeApplication(config);
                
                // Set up console event handlers for graceful shutdown
                SetupConsoleHandlers();
                
                // Start keep-alive mechanisms
                StartKeepAlive();
                
                // Run main application loop
                RunApplicationLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            finally
            {
                Cleanup();
            }
        }

        /// <summary>
        /// Configuration settings parsed from command line arguments
        /// </summary>
        public class AppConfig
        {
            public int IntervalSeconds { get; set; } = 10;
            public bool ShowHelp { get; set; } = false;
            public bool Minimized { get; set; } = false;
        }

        /// <summary>
        /// Parses command line arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Application configuration</returns>
        private static AppConfig ParseArguments(string[] args)
        {
            var config = new AppConfig();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--help":
                    case "-h":
                    case "/?":
                        config.ShowHelp = true;
                        break;
                        
                    case "--interval":
                    case "-i":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int interval))
                        {
                            config.IntervalSeconds = Math.Max(10, Math.Min(300, interval));
                            i++; // Skip next argument as it's the value
                        }
                        break;
                        
                    case "--minimized":
                    case "-m":
                        config.Minimized = true;
                        break;
                }
            }
            
            if (config.ShowHelp)
            {
                DisplayHelp();
                Environment.Exit(0);
            }
            
            return config;
        }

        /// <summary>
        /// Displays the application banner
        /// </summary>
        private static void DisplayBanner()
        {
            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         CAFFEINA                         ║");
            Console.WriteLine("║              Keep Your Computer Awake v1.0               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Prevents Windows from going to sleep, locking, or entering power-save mode.");
            Console.WriteLine();
        }

        /// <summary>
        /// Displays help information
        /// </summary>
        private static void DisplayHelp()
        {
            DisplayBanner();
            Console.WriteLine("USAGE:");
            Console.WriteLine("  Caffeina.exe [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("  --help, -h, /?           Show this help message");
            Console.WriteLine("  --interval, -i <seconds> Set keep-alive interval (10-300 seconds, default: 30)");
            Console.WriteLine("  --minimized, -m          Start minimized to system tray");
            Console.WriteLine();
            Console.WriteLine("FEATURES:");
            Console.WriteLine("  • Prevents system sleep/hibernate using SetThreadExecutionState");
            Console.WriteLine("  • Disables screensaver temporarily");
            Console.WriteLine("  • Performs subtle mouse movements (1-2 pixels)");
            Console.WriteLine("  • Sends non-intrusive keyboard events (Shift key)");
            Console.WriteLine("  • System tray icon with coffee mug");
            Console.WriteLine("  • Graceful shutdown with settings restoration");
            Console.WriteLine();
            Console.WriteLine("CONTROLS:");
            Console.WriteLine("  • Press 'Q' or Ctrl+C to quit");
            Console.WriteLine("  • Press 'S' to show status");
            Console.WriteLine("  • Press 'I' to change interval");
            Console.WriteLine("  • Right-click system tray icon for options");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  Caffeina.exe                    # Start with default 30-second interval");
            Console.WriteLine("  Caffeina.exe -i 60             # Start with 60-second interval");
            Console.WriteLine("  Caffeina.exe --minimized       # Start minimized to system tray");
            Console.WriteLine();
        }

        /// <summary>
        /// Initializes the application components
        /// </summary>
        /// <param name="config">Application configuration</param>
        private static void InitializeApplication(AppConfig config)
        {
            _startTime = DateTime.Now;
            
            // Initialize keep-alive manager
            _keepAliveManager = new KeepAliveManager(_startTime, config.IntervalSeconds);
            
            // Initialize system tray manager
            _trayManager = new SystemTrayManager(RequestExit);                    
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Application initialized successfully");
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Keep-alive interval: {config.IntervalSeconds} seconds");
            
            if (config.Minimized)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting minimized to system tray");
            }
        }

        /// <summary>
        /// Sets up console event handlers for graceful shutdown
        /// </summary>
        private static void SetupConsoleHandlers()
        {
            // Handle Ctrl+C and console close events
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate termination
                RequestExit();
            };
            
            // Handle application domain unload
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Cleanup();
            };
        }

        /// <summary>
        /// Starts the keep-alive mechanisms
        /// </summary>
        private static void StartKeepAlive()
        {
            lock (_lockObject)
            {
                if (_keepAliveManager != null && !_isRunning)
                {
                    _keepAliveManager.Start();
                    _isRunning = true;
                    
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ☕ Caffeina is now active - your computer will stay awake!");
                    ShowQuickHelp();                    
                }
            }
        }

        /// <summary>
        /// Runs the main application loop
        /// </summary>
        private static void RunApplicationLoop()
        {
            // Use a manual reset event to coordinate shutdown
            using var shutdownEvent = new ManualResetEventSlim(false);
            
            // Start message pump in background thread
            var messageLoopThread = new Thread(() =>
            {
                try
                {
                    // Run Windows Forms message loop until we signal to stop
                    while (!_exitRequested)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error in Windows Forms loop: {ex.Message}");
                }
                finally
                {
                    shutdownEvent.Set();
                }
            })
            {
                IsBackground = false,
                Name = "WinFormsMessageLoop"
            };
            messageLoopThread.SetApartmentState(ApartmentState.STA);
            messageLoopThread.Start();

            // Main console input loop
            while (!_exitRequested)
            {
                try
                {
                    // Update tray status periodically
                    UpdateTrayStatus();
                    
                    // Check for console input (non-blocking)
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        HandleUserInput(key);
                    }
                    
                    // Sleep briefly to prevent high CPU usage
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error in main loop: {ex.Message}");
                }
            }
            
            // Wait for Windows Forms thread to finish
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Waiting for Windows Forms thread to exit...");
            shutdownEvent.Wait(5000); // Wait up to 5 seconds
            
            if (messageLoopThread.IsAlive)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Force terminating Windows Forms thread...");
                messageLoopThread.Interrupt();
            }
        }

        /// <summary>
        /// Handles user keyboard input
        /// </summary>
        /// <param name="key">The key that was pressed</param>
        private static void HandleUserInput(ConsoleKeyInfo key)
        {
            switch (char.ToLower(key.KeyChar))
            {
                case 'q':
                    RequestExit();
                    break;
                    
                case 's':
                    ShowStatus();
                    break;
                    
                case 'h':
                    ShowQuickHelp();
                    break;
                    
                default:
                    // Ignore other keys
                    break;
            }
        }

        /// <summary>
        /// Shows current application status
        /// </summary>
        private static void ShowStatus()
        {
            if (_keepAliveManager != null)
            {
                var uptime = DateTime.Now - _startTime;
                var status = _keepAliveManager.IsActive ? "ACTIVE" : "INACTIVE";
                
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════════════════════╗");
                Console.WriteLine("║                 STATUS                   ║");
                Console.WriteLine("╠══════════════════════════════════════════╣");
                Console.WriteLine($"║ Keep-alive: {status,-27}  ║");
                Console.WriteLine($"║ Interval:   {_keepAliveManager.IntervalSeconds} seconds{new string(' ', 19 - _keepAliveManager.IntervalSeconds.ToString().Length)}  ║");
                Console.WriteLine($"║ Uptime:     {uptime.Days}d {uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s{new string(' ', 10)}     ║");
                Console.WriteLine($"║ Started:    {_startTime:yyyy-MM-dd HH:mm:ss}{new string(' ', 10)}║");
                Console.WriteLine("╚══════════════════════════════════════════╝");
                Console.WriteLine();
            }
        }        

        /// <summary>
        /// Shows quick help for keyboard shortcuts
        /// </summary>
        private static void ShowQuickHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("KEYBOARD SHORTCUTS:");
            Console.WriteLine("  Q - Quit application");
            Console.WriteLine("  S - Show status");            
            Console.WriteLine("  H - Show this help");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Updates the system tray status
        /// </summary>
        private static void UpdateTrayStatus()
        {
            if (_trayManager != null)
            {
                var uptime = DateTime.Now - _startTime;
                _trayManager.UpdateStatus(uptime);
            }
        }

        /// <summary>
        /// Requests application exit
        /// </summary>
        private static void RequestExit()
        {
            lock (_lockObject)
            {
                if (!_exitRequested)
                {
                    _exitRequested = true;
                    Console.WriteLine();
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exit requested - shutting down gracefully...");
                }
            }
        }

        /// <summary>
        /// Cleans up resources and exits the application
        /// </summary>
        private static void Cleanup()
        {
            lock (_lockObject)
            {
                try
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cleaning up resources...");
                    
                    // Stop keep-alive mechanisms
                    _keepAliveManager?.Stop();
                    _keepAliveManager?.Dispose();
                    
                    // Hide and dispose system tray
                    _trayManager?.Hide();
                    _trayManager?.Dispose();
                    
                    // Exit Windows Forms application
                    Application.Exit();
                    
                    _isRunning = false;
                    
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ☕ Caffeina has stopped - your computer can now sleep normally");
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] All settings have been restored");
                    Console.WriteLine("Goodbye!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error during cleanup: {ex.Message}");
                }
            }
        }
    }
}
