namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled exception assignment is in its life cycle.</summary>
public enum ScheduledExceptionAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a scheduled exception assignment and its current status.</summary>
public sealed class ScheduledExceptionAssignment(string code, string title, double minimum, string segment, string source, double budget, string country, string channel, double rating, ScheduledExceptionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public double Budget { get; set; } = budget;
    public string Country { get; set; } = country;
    public string Channel { get; set; } = channel;
    public double Rating { get; set; } = rating;
    public ScheduledExceptionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled exception assignment in status new.</summary>
    public static ScheduledExceptionAssignment NewScheduledExceptionAssignment(string code, string title)
    {
        return new ScheduledExceptionAssignment(
            code,
            title,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            0.0,
            ScheduledExceptionAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledExceptionAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledExceptionAssignmentStatus next)
    {
        switch (Status)
        {
            case ScheduledExceptionAssignmentStatus.New:
                switch (next)
                {
                    case ScheduledExceptionAssignmentStatus.Running:
                    case ScheduledExceptionAssignmentStatus.Done:
                        return true;
                }
                return false;
            case ScheduledExceptionAssignmentStatus.Queued:
                return next == ScheduledExceptionAssignmentStatus.Running || next == ScheduledExceptionAssignmentStatus.Done;
            case ScheduledExceptionAssignmentStatus.Running:
                return next == ScheduledExceptionAssignmentStatus.Failed;
            case ScheduledExceptionAssignmentStatus.Done:
                switch (next)
                {
                    case ScheduledExceptionAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledExceptionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled exception assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Budget - Minimum, 2);
    }

    public ScheduledExceptionAssignment Copy()
    {
        return new ScheduledExceptionAssignment(
            Code,
            Title,
            Minimum,
            Segment,
            Source,
            Budget,
            Country,
            Channel,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
