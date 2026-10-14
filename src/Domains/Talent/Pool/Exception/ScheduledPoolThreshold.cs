namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pool threshold is in its life cycle.</summary>
public enum ScheduledPoolThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled pool threshold, with what the planners need to act on it.</summary>
public sealed class ScheduledPoolThreshold(string code, string title, string timezone, List<string> tags, double margin, int revision, double ceiling, double rating, int attempts, ScheduledPoolThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public double Margin { get; set; } = margin;
    public int Revision { get; set; } = revision;
    public double Ceiling { get; set; } = ceiling;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public ScheduledPoolThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pool threshold in status new.</summary>
    public static ScheduledPoolThreshold NewScheduledPoolThreshold(string code, string title)
    {
        return new ScheduledPoolThreshold(
            code,
            title,
            "",
            new List<string>(),
            0.0,
            0,
            0.0,
            0.0,
            0,
            ScheduledPoolThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPoolThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPoolThresholdStatus next)
    {
        switch (Status)
        {
            case ScheduledPoolThresholdStatus.New:
                return next == ScheduledPoolThresholdStatus.Running || next == ScheduledPoolThresholdStatus.Failed;
            case ScheduledPoolThresholdStatus.Queued:
                return next == ScheduledPoolThresholdStatus.Failed;
            case ScheduledPoolThresholdStatus.Running:
                switch (next)
                {
                    case ScheduledPoolThresholdStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledPoolThresholdStatus.Done:
                switch (next)
                {
                    case ScheduledPoolThresholdStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPoolThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pool threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"scheduled pool threshold"} {Code}";
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

    public double NetValue()
    {
        return Math.Round((double)Revision + Ceiling, 2);
    }

    public ScheduledPoolThreshold Copy()
    {
        return new ScheduledPoolThreshold(
            Code,
            Title,
            Timezone,
            Tags,
            Margin,
            Revision,
            Ceiling,
            Rating,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
