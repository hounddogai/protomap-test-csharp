namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment allocation is in its life cycle.</summary>
public enum SentimentAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One sentiment allocation, with what the planners need to act on it.</summary>
public sealed class SentimentAllocation(string code, string name, bool approved, int retries, double rate, SentimentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int Retries { get; set; } = retries;
    public double Rate { get; set; } = rate;
    public SentimentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment allocation in status new.</summary>
    public static SentimentAllocation NewSentimentAllocation(string code, string name)
    {
        return new SentimentAllocation(code, name, false, 0, 0.0, SentimentAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentAllocationStatus next)
    {
        switch (Status)
        {
            case SentimentAllocationStatus.New:
                switch (next)
                {
                    case SentimentAllocationStatus.Done:
                        return true;
                }
                return false;
            case SentimentAllocationStatus.Queued:
                return next == SentimentAllocationStatus.Done;
            case SentimentAllocationStatus.Running:
                return next == SentimentAllocationStatus.Done || next == SentimentAllocationStatus.Failed;
            case SentimentAllocationStatus.Done:
                return next == SentimentAllocationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SentimentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Rate - (double)Retries, 2);
    }

    public SentimentAllocation Copy()
    {
        return new SentimentAllocation(Code, Name, Approved, Retries, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
