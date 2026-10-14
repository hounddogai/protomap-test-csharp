namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception milestone is in its life cycle.</summary>
public enum ExceptionMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One exception milestone, with what the planners need to act on it.</summary>
public sealed class ExceptionMilestone(string code, string title, int version, double fte, string employeeId, ExceptionMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public ExceptionMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a exception milestone in status new.</summary>
    public static ExceptionMilestone NewExceptionMilestone(string code, string title)
    {
        return new ExceptionMilestone(code, title, 0, 0.0, "", ExceptionMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionMilestoneStatus next)
    {
        switch (Status)
        {
            case ExceptionMilestoneStatus.New:
                return next == ExceptionMilestoneStatus.Queued;
            case ExceptionMilestoneStatus.Queued:
                return next == ExceptionMilestoneStatus.Running;
            case ExceptionMilestoneStatus.Running:
                switch (next)
                {
                    case ExceptionMilestoneStatus.Done:
                    case ExceptionMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case ExceptionMilestoneStatus.Done:
                switch (next)
                {
                    case ExceptionMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte * (double)Version, 2);
    }

    public ExceptionMilestone Copy()
    {
        return new ExceptionMilestone(Code, Title, Version, Fte, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
