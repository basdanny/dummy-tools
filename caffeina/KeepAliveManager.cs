using System.Runtime.InteropServices;
using static Caffeina.WindowsApi;

namespace Caffeina
{
    /// <summary>
    /// Manages various keep-alive mechanisms to prevent system sleep and screen lock
    /// </summary>
    public class KeepAliveManager : IDisposable
    {
        private readonly System.Threading.Timer _keepAliveTimer;
        private readonly Random _random = new();
        private readonly object _lockObject = new();
        private bool _isActive = false;
        private bool _disposed = false;
        
        // Original system settings to restore on exit
        private ExecutionState _previousExecutionState;
        
        public bool IsActive => _isActive;
        public int IntervalSeconds { get; private set; }

        public KeepAliveManager(DateTime startTime, int intervalSeconds = 10)
        {
            IntervalSeconds = intervalSeconds;
            _keepAliveTimer = new System.Threading.Timer(KeepAliveCallback, startTime, Timeout.Infinite, Timeout.Infinite);
            
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] KeepAliveManager initialized with {IntervalSeconds}s interval");
        }

        /// <summary>
        /// Starts the keep-alive mechanisms
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (_isActive || _disposed)
                    return;

                try
                {
                    // Save current execution state
                    _previousExecutionState = SetThreadExecutionState(0);

                    // Set thread execution state to prevent system sleep
                    SetThreadExecutionState(
                        ExecutionState.ES_CONTINUOUS | 
                        ExecutionState.ES_SYSTEM_REQUIRED | 
                        ExecutionState.ES_DISPLAY_REQUIRED);

                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] SetThreadExecutionState applied (previous state: {_previousExecutionState})");                                    
                    
                    // Start the keep-alive timer
                    _keepAliveTimer.Change(TimeSpan.FromSeconds(IntervalSeconds), TimeSpan.FromSeconds(IntervalSeconds));                    
                    
                    _isActive = true;
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Keep-alive mechanisms started successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error starting keep-alive mechanisms: {ex.Message}");
                    Stop(); // Ensure cleanup if anything fails
                }
            }
        }

        /// <summary>
        /// Stops the keep-alive mechanisms and restores original settings
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isActive || _disposed)
                    return;

                try
                {
                    // Stop the timer
                    _keepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    
                    // Restore original execution state
                    SetThreadExecutionState(_previousExecutionState);
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thread execution state restored");
                                        
                    _isActive = false;
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Keep-alive mechanisms stopped and settings restored");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error stopping keep-alive mechanisms: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Updates the keep-alive interval
        /// </summary>
        /// <param name="intervalSeconds">New interval in seconds (10-300)</param>
        public void UpdateInterval(int intervalSeconds)
        {
            var newInterval = Math.Max(10, Math.Min(300, intervalSeconds));
            if (newInterval == IntervalSeconds)
                return;

            lock (_lockObject)
            {
                IntervalSeconds = newInterval;
                
                if (_isActive)
                {
                    // Restart timer with new interval
                    _keepAliveTimer.Change(TimeSpan.FromSeconds(IntervalSeconds), TimeSpan.FromSeconds(IntervalSeconds));
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Keep-alive interval updated to {IntervalSeconds}s");
                }
            }
        }

        /// <summary>
        /// Timer callback that performs keep-alive actions
        /// </summary>
        /// <param name="state">Timer state (unused)</param>
        private void KeepAliveCallback(object? state)
        {
            if (!_isActive || _disposed)
                return;

            try
            {
                // Method 1: Reset execution state (most reliable)
                SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED | ExecutionState.ES_DISPLAY_REQUIRED);

                // Method 2: Subtle mouse movement (1-2 pixels)
                PerformMouseJiggle();

                // Method 3: Send non-intrusive key press (Shift key)
                SendShiftKeyPress();

                var elapsed = DateTime.Now - Convert.ToDateTime(state);

                Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\bAwake for: {0:hh\\:mm\\:ss} ☕", elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error in keep-alive callback: {ex.Message}");
            }
        }

        /// <summary>
        /// Performs a subtle mouse movement to prevent screen lock
        /// </summary>
        private void PerformMouseJiggle()
        {
            try
            {
                // Get current cursor position
                if (GetCursorPos(out WindowsApi.Point currentPos))
                {
                    // Move cursor by 1-2 pixels in a random direction
                    int deltaX = _random.Next(-2, 3);
                    int deltaY = _random.Next(-2, 3);
                    
                    // Ensure we actually move at least 1 pixel
                    if (deltaX == 0 && deltaY == 0)
                        deltaX = 1;
                    
                    var newX = currentPos.X + deltaX;
                    var newY = currentPos.Y + deltaY;
                    
                    // Move cursor to new position
                    SetCursorPos(newX, newY);
                    
                    // Move it back to original position after a short delay
                    Thread.Sleep(50);
                    SetCursorPos(currentPos.X, currentPos.Y);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Mouse jiggle failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a non-intrusive Shift key press
        /// </summary>
        private void SendShiftKeyPress()
        {
            try
            {
                var inputs = new Input[2];
                
                // Key down
                inputs[0] = new Input
                {
                    type = INPUT_KEYBOARD,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = VirtualKeyCode.VK_SHIFT,
                            dwFlags = 0
                        }
                    }
                };
                
                // Key up
                inputs[1] = new Input
                {
                    type = INPUT_KEYBOARD,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = VirtualKeyCode.VK_SHIFT,
                            dwFlags = KeyEventFlags.KEYEVENTF_KEYUP
                        }
                    }
                };

                SendInput(2, inputs, Marshal.SizeOf(typeof(Input)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Shift key press failed: {ex.Message}");
            }
        }                

        /// <summary>
        /// Disposes of resources
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _keepAliveTimer?.Dispose();
                _disposed = true;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] KeepAliveManager disposed");
            }
        }
    }
}
