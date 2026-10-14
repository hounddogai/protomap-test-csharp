namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient asset return adjustment is in its life cycle.</summary>
public enum LenientAssetReturnAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient asset return adjustment as the asset return team keeps it.</summary>
public sealed class LenientAssetReturnAdjustment(string code, string title, string notes, int limit, double discount, int priority, int createdAt, double ratio, double threshold, double ceiling, LenientAssetReturnAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public double Discount { get; set; } = discount;
    public int Priority { get; set; } = priority;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public double Threshold { get; set; } = threshold;
    public double Ceiling { get; set; } = ceiling;
    public LenientAssetReturnAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient asset return adjustment in status requested.</summary>
    public static LenientAssetReturnAdjustment NewLenientAssetReturnAdjustment(string code, string title)
    {
        return new LenientAssetReturnAdjustment(
            code,
            title,
            "",
            0,
            0.0,
            0,
            0,
            0.0,
            0.0,
            0.0,
            LenientAssetReturnAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAssetReturnAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAssetReturnAdjustmentStatus next)
    {
        switch (Status)
        {
            case LenientAssetReturnAdjustmentStatus.Requested:
                return next == LenientAssetReturnAdjustmentStatus.Granted;
            case LenientAssetReturnAdjustmentStatus.Granted:
                return next == LenientAssetReturnAdjustmentStatus.Expired;
            case LenientAssetReturnAdjustmentStatus.Revoked:
                switch (next)
                {
                    case LenientAssetReturnAdjustmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAssetReturnAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient asset return adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient asset return adjustment"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Discount * Ceiling, 2);
    }

    public LenientAssetReturnAdjustment Copy()
    {
        return new LenientAssetReturnAdjustment(
            Code,
            Title,
            Notes,
            Limit,
            Discount,
            Priority,
            CreatedAt,
            Ratio,
            Threshold,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
