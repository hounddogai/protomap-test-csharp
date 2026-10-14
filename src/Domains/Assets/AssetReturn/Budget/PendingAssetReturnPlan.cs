namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending asset return plan is in its life cycle.</summary>
public enum PendingAssetReturnPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending asset return plan as the asset return team keeps it.</summary>
public sealed class PendingAssetReturnPlan(string code, string label, string notes, string ownerId, bool active, string region, string category, double amount, PendingAssetReturnPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string OwnerId { get; set; } = ownerId;
    public bool Active { get; set; } = active;
    public string Region { get; set; } = region;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public PendingAssetReturnPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pending asset return plan in status draft.</summary>
    public static PendingAssetReturnPlan NewPendingAssetReturnPlan(string code, string label)
    {
        return new PendingAssetReturnPlan(
            code,
            label,
            "",
            "",
            false,
            "",
            "",
            0.0,
            PendingAssetReturnPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAssetReturnPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAssetReturnPlanStatus next)
    {
        switch (Status)
        {
            case PendingAssetReturnPlanStatus.Draft:
                return next == PendingAssetReturnPlanStatus.Suspended;
            case PendingAssetReturnPlanStatus.Pending:
                switch (next)
                {
                    case PendingAssetReturnPlanStatus.Active:
                    case PendingAssetReturnPlanStatus.Closed:
                        return true;
                }
                return false;
            case PendingAssetReturnPlanStatus.Active:
                return next == PendingAssetReturnPlanStatus.Closed;
            case PendingAssetReturnPlanStatus.Suspended:
                switch (next)
                {
                    case PendingAssetReturnPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingAssetReturnPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending asset return plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PendingAssetReturnPlan Copy()
    {
        return new PendingAssetReturnPlan(Code, Label, Notes, OwnerId, Active, Region, Category, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
