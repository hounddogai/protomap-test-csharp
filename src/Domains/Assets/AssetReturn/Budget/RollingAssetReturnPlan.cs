namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling asset return plan is in its life cycle.</summary>
public enum RollingAssetReturnPlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling asset return plan, with what the planners need to act on it.</summary>
public sealed class RollingAssetReturnPlan(string code, string label, string phone, int endsAt, int retries, int graceDays, string badge, string unit, RollingAssetReturnPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public int GraceDays { get; set; } = graceDays;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public RollingAssetReturnPlanStatus Status { get; set; } = status;

    /// <summary>Starts a rolling asset return plan in status planned.</summary>
    public static RollingAssetReturnPlan NewRollingAssetReturnPlan(string code, string label)
    {
        return new RollingAssetReturnPlan(code, label, "", 0, 0, 0, "", "", RollingAssetReturnPlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAssetReturnPlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAssetReturnPlanStatus next)
    {
        switch (Status)
        {
            case RollingAssetReturnPlanStatus.Planned:
                return next == RollingAssetReturnPlanStatus.Scheduled || next == RollingAssetReturnPlanStatus.Cancelled;
            case RollingAssetReturnPlanStatus.Scheduled:
                return next == RollingAssetReturnPlanStatus.InProgress || next == RollingAssetReturnPlanStatus.Completed;
            case RollingAssetReturnPlanStatus.InProgress:
                return next == RollingAssetReturnPlanStatus.Completed || next == RollingAssetReturnPlanStatus.Cancelled;
            case RollingAssetReturnPlanStatus.Completed:
                return next == RollingAssetReturnPlanStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingAssetReturnPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling asset return plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"rolling asset return plan"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Retries * (double)EndsAt, 2);
    }

    public RollingAssetReturnPlan Copy()
    {
        return new RollingAssetReturnPlan(Code, Label, Phone, EndsAt, Retries, GraceDays, Badge, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
