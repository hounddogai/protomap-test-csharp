namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global assignment adjustment is in its life cycle.</summary>
public enum GlobalAssignmentAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One global assignment adjustment, with what the planners need to act on it.</summary>
public sealed class GlobalAssignmentAdjustment(string code, string label, int tenureMonths, int expiresAt, double amount, bool active, string skill, string costCenter, int priority, GlobalAssignmentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Amount { get; set; } = amount;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public string CostCenter { get; set; } = costCenter;
    public int Priority { get; set; } = priority;
    public GlobalAssignmentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a global assignment adjustment in status planned.</summary>
    public static GlobalAssignmentAdjustment NewGlobalAssignmentAdjustment(string code, string label)
    {
        return new GlobalAssignmentAdjustment(
            code,
            label,
            0,
            0,
            0.0,
            false,
            "",
            "",
            0,
            GlobalAssignmentAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAssignmentAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAssignmentAdjustmentStatus next)
    {
        switch (Status)
        {
            case GlobalAssignmentAdjustmentStatus.Planned:
                return next == GlobalAssignmentAdjustmentStatus.Scheduled || next == GlobalAssignmentAdjustmentStatus.InProgress;
            case GlobalAssignmentAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case GlobalAssignmentAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case GlobalAssignmentAdjustmentStatus.InProgress:
                return next == GlobalAssignmentAdjustmentStatus.Completed || next == GlobalAssignmentAdjustmentStatus.Cancelled;
            case GlobalAssignmentAdjustmentStatus.Completed:
                return next == GlobalAssignmentAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalAssignmentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global assignment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt - Amount, 2);
    }

    public GlobalAssignmentAdjustment Copy()
    {
        return new GlobalAssignmentAdjustment(
            Code,
            Label,
            TenureMonths,
            ExpiresAt,
            Amount,
            Active,
            Skill,
            CostCenter,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
