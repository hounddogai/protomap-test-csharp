namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional statement of work milestone is in its life cycle.</summary>
public enum RegionalStatementOfWorkMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional statement of work milestone. Its code is unique within a tenant.</summary>
public sealed class RegionalStatementOfWorkMilestone(string code, string title, string badge, string name, int updatedAt, bool archived, string channel, string employeeId, double weight, RegionalStatementOfWorkMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public string EmployeeId { get; set; } = employeeId;
    public double Weight { get; set; } = weight;
    public RegionalStatementOfWorkMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a regional statement of work milestone in status proposed.</summary>
    public static RegionalStatementOfWorkMilestone NewRegionalStatementOfWorkMilestone(string code, string title)
    {
        return new RegionalStatementOfWorkMilestone(
            code,
            title,
            "",
            "",
            0,
            false,
            "",
            "",
            0.0,
            RegionalStatementOfWorkMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalStatementOfWorkMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalStatementOfWorkMilestoneStatus next)
    {
        switch (Status)
        {
            case RegionalStatementOfWorkMilestoneStatus.Proposed:
                switch (next)
                {
                    case RegionalStatementOfWorkMilestoneStatus.Rejected:
                        return true;
                }
                return false;
            case RegionalStatementOfWorkMilestoneStatus.Approved:
                switch (next)
                {
                    case RegionalStatementOfWorkMilestoneStatus.Rejected:
                    case RegionalStatementOfWorkMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalStatementOfWorkMilestoneStatus.Rejected:
                return next == RegionalStatementOfWorkMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalStatementOfWorkMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional statement of work milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Weight + (double)UpdatedAt, 2);
    }

    public RegionalStatementOfWorkMilestone Copy()
    {
        return new RegionalStatementOfWorkMilestone(
            Code,
            Title,
            Badge,
            Name,
            UpdatedAt,
            Archived,
            Channel,
            EmployeeId,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
