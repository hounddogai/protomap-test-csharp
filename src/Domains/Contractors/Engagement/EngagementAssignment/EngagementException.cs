namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement exception is in its life cycle.</summary>
public enum EngagementExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One engagement exception, with what the planners need to act on it.</summary>
public sealed class EngagementException(string code, string title, double ceiling, string checksum, string locale, int capacity, string team, double maximum, string summary, double rate, EngagementExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Checksum { get; set; } = checksum;
    public string Locale { get; set; } = locale;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public double Rate { get; set; } = rate;
    public EngagementExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a engagement exception in status planned.</summary>
    public static EngagementException NewEngagementException(string code, string title)
    {
        return new EngagementException(
            code,
            title,
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            "",
            0.0,
            EngagementExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementExceptionStatus next)
    {
        switch (Status)
        {
            case EngagementExceptionStatus.Planned:
                switch (next)
                {
                    case EngagementExceptionStatus.Scheduled:
                    case EngagementExceptionStatus.InProgress:
                        return true;
                }
                return false;
            case EngagementExceptionStatus.Scheduled:
                switch (next)
                {
                    case EngagementExceptionStatus.Completed:
                    case EngagementExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case EngagementExceptionStatus.InProgress:
                switch (next)
                {
                    case EngagementExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case EngagementExceptionStatus.Completed:
                return next == EngagementExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EngagementExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Maximum + Ceiling, 2);
    }

    public EngagementException Copy()
    {
        return new EngagementException(
            Code,
            Title,
            Ceiling,
            Checksum,
            Locale,
            Capacity,
            Team,
            Maximum,
            Summary,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
