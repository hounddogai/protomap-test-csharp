namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive adjustment claim is in its life cycle.</summary>
public enum AdaptiveAdjustmentClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive adjustment claim as the adjustment team keeps it.</summary>
public sealed class AdaptiveAdjustmentClaim(string code, string title, double amount, string locale, double score, string location, string region, double ratio, string name, int tenureMonths, string notes, AdaptiveAdjustmentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Locale { get; set; } = locale;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public string Region { get; set; } = region;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Notes { get; set; } = notes;
    public AdaptiveAdjustmentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive adjustment claim in status created.</summary>
    public static AdaptiveAdjustmentClaim NewAdaptiveAdjustmentClaim(string code, string title)
    {
        return new AdaptiveAdjustmentClaim(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            0,
            "",
            AdaptiveAdjustmentClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAdjustmentClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAdjustmentClaimStatus next)
    {
        switch (Status)
        {
            case AdaptiveAdjustmentClaimStatus.Created:
                return next == AdaptiveAdjustmentClaimStatus.Validated || next == AdaptiveAdjustmentClaimStatus.Published;
            case AdaptiveAdjustmentClaimStatus.Validated:
                return next == AdaptiveAdjustmentClaimStatus.Published || next == AdaptiveAdjustmentClaimStatus.Retired;
            case AdaptiveAdjustmentClaimStatus.Published:
                return next == AdaptiveAdjustmentClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveAdjustmentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive adjustment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount * (double)TenureMonths, 2);
    }

    public AdaptiveAdjustmentClaim Copy()
    {
        return new AdaptiveAdjustmentClaim(
            Code,
            Title,
            Amount,
            Locale,
            Score,
            Location,
            Region,
            Ratio,
            Name,
            TenureMonths,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
