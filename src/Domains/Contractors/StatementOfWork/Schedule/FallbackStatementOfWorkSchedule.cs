namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback statement of work schedule is in its life cycle.</summary>
public enum FallbackStatementOfWorkScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a fallback statement of work schedule and its current status.</summary>
public sealed class FallbackStatementOfWorkSchedule(string code, string title, List<string> tags, string name, string description, double threshold, double maximum, int priority, FallbackStatementOfWorkScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public double Maximum { get; set; } = maximum;
    public int Priority { get; set; } = priority;
    public FallbackStatementOfWorkScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback statement of work schedule in status planned.</summary>
    public static FallbackStatementOfWorkSchedule NewFallbackStatementOfWorkSchedule(string code, string title)
    {
        return new FallbackStatementOfWorkSchedule(
            code,
            title,
            new List<string>(),
            "",
            "",
            0.0,
            0.0,
            0,
            FallbackStatementOfWorkScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackStatementOfWorkScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackStatementOfWorkScheduleStatus next)
    {
        switch (Status)
        {
            case FallbackStatementOfWorkScheduleStatus.Planned:
                return next == FallbackStatementOfWorkScheduleStatus.Scheduled || next == FallbackStatementOfWorkScheduleStatus.Completed;
            case FallbackStatementOfWorkScheduleStatus.Scheduled:
                switch (next)
                {
                    case FallbackStatementOfWorkScheduleStatus.InProgress:
                    case FallbackStatementOfWorkScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackStatementOfWorkScheduleStatus.InProgress:
                switch (next)
                {
                    case FallbackStatementOfWorkScheduleStatus.Completed:
                        return true;
                }
                return false;
            case FallbackStatementOfWorkScheduleStatus.Completed:
                return next == FallbackStatementOfWorkScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackStatementOfWorkScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback statement of work schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double ProjectedValue()
    {
        return Math.Round(Maximum * (double)Priority, 2);
    }

    public FallbackStatementOfWorkSchedule Copy()
    {
        return new FallbackStatementOfWorkSchedule(
            Code,
            Title,
            Tags,
            Name,
            Description,
            Threshold,
            Maximum,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
