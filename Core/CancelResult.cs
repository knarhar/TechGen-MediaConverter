namespace Core
{
    public enum CancelResult
    {
        NotFound,
        CanceledQueued,
        CanceledRunning,
        AlreadyFinished
    }
}
