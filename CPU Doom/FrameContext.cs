namespace CPU_Doom
{
    /// <summary>
    /// Provides access to the current frame context including time management.
    /// </summary>
    public interface IUserFrameContext
    {
        /// <summary>
        /// Gets the time management interface for the current frame context.
        /// </summary>
        IUserWindowTime Time {  get; } 
    }

    /// <summary>
    /// Represents the frame context, which includes time management functionalities.
    /// </summary>
    internal class FrameContext : IUserFrameContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameContext"/> class.
        /// </summary>
        /// <param name="time">The time management instance to use.</param>
        internal FrameContext(WidnowTime time) 
        {
            _time = time;
        }

        /// <summary>
        /// Updates context to next frame
        /// </summary>
        public void NextFrame()
        {
            _time.NextFrame();
        }
        private WidnowTime _time;

        /// <summary>
        /// Gets the time management interface associated with this frame context.
        /// </summary>
        public IUserWindowTime Time => _time;
    }

    /// <summary>
    /// Provides an interface for accessing time-related information in the window.
    /// </summary>
    public interface IUserWindowTime 
    {
        /// <summary>
        /// Gets the current time in seconds since the application started.
        /// </summary>
        double Time { get; }

        /// <summary>
        /// Gets the time elapsed since the last frame in seconds.
        /// </summary>
        double DeltaTime { get; }
    }

    /// <summary>
    /// Implements time management for the window, tracking current time and frame time.
    /// </summary>
    internal class WidnowTime : IUserWindowTime
    {
        /// <summary>
        /// Gets the current time in seconds since the application started.
        /// </summary>
        public double Time => (DateTime.Now - _initTime).TotalMilliseconds / 1000f;

        /// <summary>
        /// Gets the time elapsed since the last frame in seconds.
        /// </summary>
        public double DeltaTime { get; private set; }

        /// <summary>
        /// Advances to the next frame and updates the time elapsed since the last frame.
        /// </summary>
        public void NextFrame()
        {
            DateTime now = DateTime.Now;
            DeltaTime = (now - _lastFrameTime).TotalMilliseconds / 1000f;
            _lastFrameTime = now;
        }

        private DateTime _lastFrameTime = DateTime.Now;
        private DateTime _initTime = DateTime.Now;
    }
}
