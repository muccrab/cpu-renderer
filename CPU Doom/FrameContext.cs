namespace CPU_Doom
{
    public interface IUserFrameContext
    {
        IUserWindowTime Time {  get; } 
    }
    internal class FrameContext : IUserFrameContext
    {
        internal FrameContext(WidnowTime time) 
        {
            _time = time;
        }
        public void NextFrame()
        {
            _time.NextFrame();
        }
        private WidnowTime _time;
        public IUserWindowTime Time => _time;
    }

    // Interface For User Interaction of Timer
    public interface IUserWindowTime 
    {
        double Time { get; }
        double DeltaTime { get; }
    }

    // Internal Class For Window Controller
    internal class WidnowTime : IUserWindowTime
    {
        public double Time => (DateTime.Now - _initTime).TotalMilliseconds / 1000f;
        public double DeltaTime { get; private set; }
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
