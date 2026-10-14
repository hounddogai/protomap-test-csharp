namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool milestone is in its life cycle.</summary>
public enum BonusPoolMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A bonus pool milestone as the bonus pool team keeps it.</summary>
public sealed class BonusPoolMilestone(string code, string title, int updatedAt, double rate, string costCenter, double amount, string employeeId, string summary, string phone, double hours, double floorValue, BonusPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rate { get; set; } = rate;
    public string CostCenter { get; set; } = costCenter;
    public double Amount { get; set; } = amount;
    public string EmployeeId { get; set; } = employeeId;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public double Hours { get; set; } = hours;
    public double FloorValue { get; set; } = floorValue;
    public BonusPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool milestone in status open.</summary>
    public static BonusPoolMilestone NewBonusPoolMilestone(string code, string title)
    {
        return new BonusPoolMilestone(
            code,
            title,
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            BonusPoolMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case BonusPoolMilestoneStatus.Open:
                switch (next)
                {
                    case BonusPoolMilestoneStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolMilestoneStatus.InReview:
                return next == BonusPoolMilestoneStatus.Resolved || next == BonusPoolMilestoneStatus.Reopened;
            case BonusPoolMilestoneStatus.Resolved:
                return next == BonusPoolMilestoneStatus.Reopened || next == BonusPoolMilestoneStatus.Archived;
            case BonusPoolMilestoneStatus.Reopened:
                switch (next)
                {
                    case BonusPoolMilestoneStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)UpdatedAt - FloorValue, 2);
    }

    public BonusPoolMilestone Copy()
    {
        return new BonusPoolMilestone(
            Code,
            Title,
            UpdatedAt,
            Rate,
            CostCenter,
            Amount,
            EmployeeId,
            Summary,
            Phone,
            Hours,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
