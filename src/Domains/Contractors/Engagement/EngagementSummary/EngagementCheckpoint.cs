namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement checkpoint is in its life cycle.</summary>
public enum EngagementCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A engagement checkpoint. Its code is unique within a tenant.</summary>
public sealed class EngagementCheckpoint(string code, string title, string jobCode, string managerId, string department, string salaryBand, string label, int priority, string region, EngagementCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string ManagerId { get; set; } = managerId;
    public string Department { get; set; } = department;
    public string SalaryBand { get; set; } = salaryBand;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Region { get; set; } = region;
    public EngagementCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a engagement checkpoint in status proposed.</summary>
    public static EngagementCheckpoint NewEngagementCheckpoint(string code, string title)
    {
        return new EngagementCheckpoint(code, title, "", "", "", "", "", 0, "", EngagementCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementCheckpointStatus next)
    {
        switch (Status)
        {
            case EngagementCheckpointStatus.Proposed:
                return next == EngagementCheckpointStatus.Approved || next == EngagementCheckpointStatus.Rejected;
            case EngagementCheckpointStatus.Approved:
                switch (next)
                {
                    case EngagementCheckpointStatus.Rejected:
                    case EngagementCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case EngagementCheckpointStatus.Rejected:
                switch (next)
                {
                    case EngagementCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public EngagementCheckpoint Copy()
    {
        return new EngagementCheckpoint(
            Code,
            Title,
            JobCode,
            ManagerId,
            Department,
            SalaryBand,
            Label,
            Priority,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
