namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped asset return plan is in its life cycle.</summary>
public enum CappedAssetReturnPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped asset return plan and its current status.</summary>
public sealed class CappedAssetReturnPlan(string code, string name, string notes, string segment, string label, double amount, CappedAssetReturnPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Segment { get; set; } = segment;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public CappedAssetReturnPlanStatus Status { get; set; } = status;

    /// <summary>Starts a capped asset return plan in status new.</summary>
    public static CappedAssetReturnPlan NewCappedAssetReturnPlan(string code, string name)
    {
        return new CappedAssetReturnPlan(code, name, "", "", "", 0.0, CappedAssetReturnPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAssetReturnPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAssetReturnPlanStatus next)
    {
        switch (Status)
        {
            case CappedAssetReturnPlanStatus.New:
                return next == CappedAssetReturnPlanStatus.Failed;
            case CappedAssetReturnPlanStatus.Queued:
                switch (next)
                {
                    case CappedAssetReturnPlanStatus.Failed:
                        return true;
                }
                return false;
            case CappedAssetReturnPlanStatus.Running:
                return next == CappedAssetReturnPlanStatus.Done;
            case CappedAssetReturnPlanStatus.Done:
                switch (next)
                {
                    case CappedAssetReturnPlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAssetReturnPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped asset return plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public CappedAssetReturnPlan Copy()
    {
        return new CappedAssetReturnPlan(Code, Name, Notes, Segment, Label, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
