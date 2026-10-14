namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal asset return quota is in its life cycle.</summary>
public enum SeasonalAssetReturnQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a seasonal asset return quota and its current status.</summary>
public sealed class SeasonalAssetReturnQuota(string code, string title, string costCenter, string reviewer, double ratio, SeasonalAssetReturnQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string Reviewer { get; set; } = reviewer;
    public double Ratio { get; set; } = ratio;
    public SeasonalAssetReturnQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal asset return quota in status new.</summary>
    public static SeasonalAssetReturnQuota NewSeasonalAssetReturnQuota(string code, string title)
    {
        return new SeasonalAssetReturnQuota(code, title, "", "", 0.0, SeasonalAssetReturnQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAssetReturnQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAssetReturnQuotaStatus next)
    {
        switch (Status)
        {
            case SeasonalAssetReturnQuotaStatus.New:
                return next == SeasonalAssetReturnQuotaStatus.Failed;
            case SeasonalAssetReturnQuotaStatus.Queued:
                return next == SeasonalAssetReturnQuotaStatus.Running;
            case SeasonalAssetReturnQuotaStatus.Running:
                return next == SeasonalAssetReturnQuotaStatus.Done;
            case SeasonalAssetReturnQuotaStatus.Done:
                return next == SeasonalAssetReturnQuotaStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalAssetReturnQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal asset return quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public SeasonalAssetReturnQuota Copy()
    {
        return new SeasonalAssetReturnQuota(Code, Title, CostCenter, Reviewer, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
