namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary leave request milestone is in its life cycle.</summary>
public enum PrimaryLeaveRequestMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One primary leave request milestone, with what the planners need to act on it.</summary>
public sealed class PrimaryLeaveRequestMilestone(string code, string title, int windowDays, int batchSize, string salaryBand, int retries, double amount, double maximum, string summary, int priority, PrimaryLeaveRequestMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int BatchSize { get; set; } = batchSize;
    public string SalaryBand { get; set; } = salaryBand;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public double Maximum { get; set; } = maximum;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public PrimaryLeaveRequestMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a primary leave request milestone in status idle.</summary>
    public static PrimaryLeaveRequestMilestone NewPrimaryLeaveRequestMilestone(string code, string title)
    {
        return new PrimaryLeaveRequestMilestone(
            code,
            title,
            0,
            0,
            "",
            0,
            0.0,
            0.0,
            "",
            0,
            PrimaryLeaveRequestMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLeaveRequestMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLeaveRequestMilestoneStatus next)
    {
        switch (Status)
        {
            case PrimaryLeaveRequestMilestoneStatus.Idle:
                return next == PrimaryLeaveRequestMilestoneStatus.Draining;
            case PrimaryLeaveRequestMilestoneStatus.Warming:
                return next == PrimaryLeaveRequestMilestoneStatus.Draining;
            case PrimaryLeaveRequestMilestoneStatus.Ready:
                return next == PrimaryLeaveRequestMilestoneStatus.Draining || next == PrimaryLeaveRequestMilestoneStatus.Stopped;
            case PrimaryLeaveRequestMilestoneStatus.Draining:
                return next == PrimaryLeaveRequestMilestoneStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryLeaveRequestMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary leave request milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)BatchSize * (double)Priority, 2);
    }

    public PrimaryLeaveRequestMilestone Copy()
    {
        return new PrimaryLeaveRequestMilestone(
            Code,
            Title,
            WindowDays,
            BatchSize,
            SalaryBand,
            Retries,
            Amount,
            Maximum,
            Summary,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
