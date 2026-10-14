namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy asset return plan is in its life cycle.</summary>
public enum LegacyAssetReturnPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a legacy asset return plan and its current status.</summary>
public sealed class LegacyAssetReturnPlan(string code, string title, string ownerId, string managerId, double discount, string jobCode, LegacyAssetReturnPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public string JobCode { get; set; } = jobCode;
    public LegacyAssetReturnPlanStatus Status { get; set; } = status;

    /// <summary>Starts a legacy asset return plan in status open.</summary>
    public static LegacyAssetReturnPlan NewLegacyAssetReturnPlan(string code, string title)
    {
        return new LegacyAssetReturnPlan(code, title, "", "", 0.0, "", LegacyAssetReturnPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAssetReturnPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAssetReturnPlanStatus next)
    {
        switch (Status)
        {
            case LegacyAssetReturnPlanStatus.Open:
                switch (next)
                {
                    case LegacyAssetReturnPlanStatus.InReview:
                    case LegacyAssetReturnPlanStatus.Archived:
                        return true;
                }
                return false;
            case LegacyAssetReturnPlanStatus.InReview:
                switch (next)
                {
                    case LegacyAssetReturnPlanStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyAssetReturnPlanStatus.Resolved:
                return next == LegacyAssetReturnPlanStatus.Archived;
            case LegacyAssetReturnPlanStatus.Reopened:
                return next == LegacyAssetReturnPlanStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyAssetReturnPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy asset return plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public LegacyAssetReturnPlan Copy()
    {
        return new LegacyAssetReturnPlan(Code, Title, OwnerId, ManagerId, Discount, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
