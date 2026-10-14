namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted asset return quota is in its life cycle.</summary>
public enum WeightedAssetReturnQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a weighted asset return quota and its current status.</summary>
public sealed class WeightedAssetReturnQuota(string code, string title, double budget, bool approved, string unit, string checksum, WeightedAssetReturnQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public bool Approved { get; set; } = approved;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public WeightedAssetReturnQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a weighted asset return quota in status open.</summary>
    public static WeightedAssetReturnQuota NewWeightedAssetReturnQuota(string code, string title)
    {
        return new WeightedAssetReturnQuota(code, title, 0.0, false, "", "", WeightedAssetReturnQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAssetReturnQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAssetReturnQuotaStatus next)
    {
        switch (Status)
        {
            case WeightedAssetReturnQuotaStatus.Open:
                switch (next)
                {
                    case WeightedAssetReturnQuotaStatus.InReview:
                    case WeightedAssetReturnQuotaStatus.Resolved:
                        return true;
                }
                return false;
            case WeightedAssetReturnQuotaStatus.InReview:
                return next == WeightedAssetReturnQuotaStatus.Resolved;
            case WeightedAssetReturnQuotaStatus.Resolved:
                return next == WeightedAssetReturnQuotaStatus.Reopened || next == WeightedAssetReturnQuotaStatus.Archived;
            case WeightedAssetReturnQuotaStatus.Reopened:
                switch (next)
                {
                    case WeightedAssetReturnQuotaStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAssetReturnQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted asset return quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public WeightedAssetReturnQuota Copy()
    {
        return new WeightedAssetReturnQuota(Code, Title, Budget, Approved, Unit, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
