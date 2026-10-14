namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback asset return threshold is in its life cycle.</summary>
public enum FallbackAssetReturnThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback asset return threshold. Its code is unique within a tenant.</summary>
public sealed class FallbackAssetReturnThreshold(string code, string label, string jobCode, double ceiling, double maximum, FallbackAssetReturnThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Ceiling { get; set; } = ceiling;
    public double Maximum { get; set; } = maximum;
    public FallbackAssetReturnThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a fallback asset return threshold in status open.</summary>
    public static FallbackAssetReturnThreshold NewFallbackAssetReturnThreshold(string code, string label)
    {
        return new FallbackAssetReturnThreshold(code, label, "", 0.0, 0.0, FallbackAssetReturnThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAssetReturnThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAssetReturnThresholdStatus next)
    {
        switch (Status)
        {
            case FallbackAssetReturnThresholdStatus.Open:
                return next == FallbackAssetReturnThresholdStatus.InReview;
            case FallbackAssetReturnThresholdStatus.InReview:
                return next == FallbackAssetReturnThresholdStatus.Reopened;
            case FallbackAssetReturnThresholdStatus.Resolved:
                switch (next)
                {
                    case FallbackAssetReturnThresholdStatus.Reopened:
                    case FallbackAssetReturnThresholdStatus.Archived:
                        return true;
                }
                return false;
            case FallbackAssetReturnThresholdStatus.Reopened:
                return next == FallbackAssetReturnThresholdStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackAssetReturnThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback asset return threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback asset return threshold"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Ceiling * Maximum, 2);
    }

    public FallbackAssetReturnThreshold Copy()
    {
        return new FallbackAssetReturnThreshold(Code, Label, JobCode, Ceiling, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
