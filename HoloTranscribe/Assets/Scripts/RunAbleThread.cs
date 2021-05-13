using System.Threading;


public abstract class RunAbleThread
{
    private Thread Thread;
    private byte[] stream;
    private string input;

    protected RunAbleThread()
    {
        // Create a thread instead of calling Run() directly because it would block unity from doing other tasks.
        Thread = new Thread(Run);
    }

    protected bool Running { get; private set; }


    /// This method will get called when you call Start().
    protected abstract void Run();

    public void Start()
    {
        //We are running a request.
        Running = true;    
        Thread.Start();
    }


    public void Stop()
    {
        // block main thread, wait for thread runner to finish job first.
        Thread.Join();
        Running = false;
    }
}