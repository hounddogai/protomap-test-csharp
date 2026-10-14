namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled sentiment target is in its life cycle.</summary>
public enum ScheduledSentimentTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scheduled sentiment target and its current status.</summary>
public sealed class ScheduledSentimentTarget(string code, string name, int updatedAt, int graceDays, int limit, List<string> tags, string source, ScheduledSentimentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public ScheduledSentimentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled sentiment target in status idle.</summary>
    public static ScheduledSentimentTarget NewScheduledSentimentTarget(string code, string name)
    {
        return new ScheduledSentimentTarget(
            code,
            name,
            0,
            0,
            0,
            new List<string>(),
            "",
            ScheduledSentimentTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSentimentTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSentimentTargetStatus next)
    {
        switch (Status)
        {
            case ScheduledSentimentTargetStatus.Idle:
                switch (next)
                {
                    case ScheduledSentimentTargetStatus.Warming:
                        return true;
                }
                return false;
            case ScheduledSentimentTargetStatus.Warming:
                switch (next)
                {
                    case ScheduledSentimentTargetStatus.Ready:
                        return true;
                }
                return false;
            case ScheduledSentimentTargetStatus.Ready:
                return next == ScheduledSentimentTargetStatus.Draining || next == ScheduledSentimentTargetStatus.Stopped;
            case ScheduledSentimentTargetStatus.Draining:
                switch (next)
                {
                    case ScheduledSentimentTargetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledSentimentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled sentiment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled sentiment target"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Limit * (double)UpdatedAt, 2);
    }

    public ScheduledSentimentTarget Copy()
    {
        return new ScheduledSentimentTarget(Code, Name, UpdatedAt, GraceDays, Limit, Tags, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
