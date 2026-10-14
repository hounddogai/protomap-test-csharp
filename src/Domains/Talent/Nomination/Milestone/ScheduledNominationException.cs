namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled nomination exception is in its life cycle.</summary>
public enum ScheduledNominationExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a scheduled nomination exception and its current status.</summary>
public sealed class ScheduledNominationException(string code, string title, double maximum, List<string> tags, string managerId, ScheduledNominationExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public string ManagerId { get; set; } = managerId;
    public ScheduledNominationExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled nomination exception in status new.</summary>
    public static ScheduledNominationException NewScheduledNominationException(string code, string title)
    {
        return new ScheduledNominationException(
            code,
            title,
            0.0,
            new List<string>(),
            "",
            ScheduledNominationExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledNominationExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledNominationExceptionStatus next)
    {
        switch (Status)
        {
            case ScheduledNominationExceptionStatus.New:
                switch (next)
                {
                    case ScheduledNominationExceptionStatus.Done:
                        return true;
                }
                return false;
            case ScheduledNominationExceptionStatus.Queued:
                switch (next)
                {
                    case ScheduledNominationExceptionStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledNominationExceptionStatus.Running:
                switch (next)
                {
                    case ScheduledNominationExceptionStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledNominationExceptionStatus.Done:
                switch (next)
                {
                    case ScheduledNominationExceptionStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledNominationExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled nomination exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled nomination exception"} {Code}";
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

    public double MeasuredValue()
    {
        return Maximum;
    }

    public ScheduledNominationException Copy()
    {
        return new ScheduledNominationException(Code, Title, Maximum, Tags, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
