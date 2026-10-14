namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment segment is in its life cycle.</summary>
public enum SentimentSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One sentiment segment, with what the planners need to act on it.</summary>
public sealed class SentimentSegment(string code, string name, bool archived, string team, string phone, double amount, SentimentSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public SentimentSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment segment in status new.</summary>
    public static SentimentSegment NewSentimentSegment(string code, string name)
    {
        return new SentimentSegment(code, name, false, "", "", 0.0, SentimentSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentSegmentStatus next)
    {
        switch (Status)
        {
            case SentimentSegmentStatus.New:
                return next == SentimentSegmentStatus.Queued;
            case SentimentSegmentStatus.Queued:
                switch (next)
                {
                    case SentimentSegmentStatus.Failed:
                        return true;
                }
                return false;
            case SentimentSegmentStatus.Running:
                switch (next)
                {
                    case SentimentSegmentStatus.Done:
                    case SentimentSegmentStatus.Failed:
                        return true;
                }
                return false;
            case SentimentSegmentStatus.Done:
                switch (next)
                {
                    case SentimentSegmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public SentimentSegment Copy()
    {
        return new SentimentSegment(Code, Name, Archived, Team, Phone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
