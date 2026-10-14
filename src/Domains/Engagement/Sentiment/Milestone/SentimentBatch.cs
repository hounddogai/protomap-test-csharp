namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment batch is in its life cycle.</summary>
public enum SentimentBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One sentiment batch, with what the planners need to act on it.</summary>
public sealed class SentimentBatch(string code, string title, int limit, string phone, string segment, string grade, int leaveDays, string source, int windowDays, double cost, SentimentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public string Grade { get; set; } = grade;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public int WindowDays { get; set; } = windowDays;
    public double Cost { get; set; } = cost;
    public SentimentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment batch in status idle.</summary>
    public static SentimentBatch NewSentimentBatch(string code, string title)
    {
        return new SentimentBatch(code, title, 0, "", "", "", 0, "", 0, 0.0, SentimentBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentBatchStatus next)
    {
        switch (Status)
        {
            case SentimentBatchStatus.Idle:
                return next == SentimentBatchStatus.Draining || next == SentimentBatchStatus.Stopped;
            case SentimentBatchStatus.Warming:
                switch (next)
                {
                    case SentimentBatchStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentBatchStatus.Ready:
                switch (next)
                {
                    case SentimentBatchStatus.Draining:
                    case SentimentBatchStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentBatchStatus.Draining:
                return next == SentimentBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SentimentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment batch"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Limit - (double)WindowDays, 2);
    }

    public SentimentBatch Copy()
    {
        return new SentimentBatch(
            Code,
            Title,
            Limit,
            Phone,
            Segment,
            Grade,
            LeaveDays,
            Source,
            WindowDays,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
