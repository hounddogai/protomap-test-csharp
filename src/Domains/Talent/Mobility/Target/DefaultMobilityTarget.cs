namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default mobility target is in its life cycle.</summary>
public enum DefaultMobilityTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A default mobility target. Its code is unique within a tenant.</summary>
public sealed class DefaultMobilityTarget(string code, string title, string department, int leaveDays, string employeeId, int windowDays, int retries, string reviewer, string label, string skill, double rating, DefaultMobilityTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int LeaveDays { get; set; } = leaveDays;
    public string EmployeeId { get; set; } = employeeId;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public string Reviewer { get; set; } = reviewer;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public DefaultMobilityTargetStatus Status { get; set; } = status;

    /// <summary>Starts a default mobility target in status idle.</summary>
    public static DefaultMobilityTarget NewDefaultMobilityTarget(string code, string title)
    {
        return new DefaultMobilityTarget(
            code,
            title,
            "",
            0,
            "",
            0,
            0,
            "",
            "",
            "",
            0.0,
            DefaultMobilityTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultMobilityTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultMobilityTargetStatus next)
    {
        switch (Status)
        {
            case DefaultMobilityTargetStatus.Idle:
                return next == DefaultMobilityTargetStatus.Ready || next == DefaultMobilityTargetStatus.Draining;
            case DefaultMobilityTargetStatus.Warming:
                return next == DefaultMobilityTargetStatus.Draining;
            case DefaultMobilityTargetStatus.Ready:
                return next == DefaultMobilityTargetStatus.Draining || next == DefaultMobilityTargetStatus.Stopped;
            case DefaultMobilityTargetStatus.Draining:
                return next == DefaultMobilityTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DefaultMobilityTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default mobility target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)LeaveDays + (double)Retries, 2);
    }

    public DefaultMobilityTarget Copy()
    {
        return new DefaultMobilityTarget(
            Code,
            Title,
            Department,
            LeaveDays,
            EmployeeId,
            WindowDays,
            Retries,
            Reviewer,
            Label,
            Skill,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
