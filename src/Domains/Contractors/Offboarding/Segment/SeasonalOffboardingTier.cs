namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal offboarding tier is in its life cycle.</summary>
public enum SeasonalOffboardingTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal offboarding tier as the offboarding team keeps it.</summary>
public sealed class SeasonalOffboardingTier(string code, string label, string externalRef, string salaryBand, int windowDays, int quantity, double threshold, string reviewer, double ratio, string region, SeasonalOffboardingTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string SalaryBand { get; set; } = salaryBand;
    public int WindowDays { get; set; } = windowDays;
    public int Quantity { get; set; } = quantity;
    public double Threshold { get; set; } = threshold;
    public string Reviewer { get; set; } = reviewer;
    public double Ratio { get; set; } = ratio;
    public string Region { get; set; } = region;
    public SeasonalOffboardingTierStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal offboarding tier in status proposed.</summary>
    public static SeasonalOffboardingTier NewSeasonalOffboardingTier(string code, string label)
    {
        return new SeasonalOffboardingTier(
            code,
            label,
            "",
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            "",
            SeasonalOffboardingTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalOffboardingTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalOffboardingTierStatus next)
    {
        switch (Status)
        {
            case SeasonalOffboardingTierStatus.Proposed:
                return next == SeasonalOffboardingTierStatus.Approved || next == SeasonalOffboardingTierStatus.Withdrawn;
            case SeasonalOffboardingTierStatus.Approved:
                switch (next)
                {
                    case SeasonalOffboardingTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalOffboardingTierStatus.Rejected:
                return next == SeasonalOffboardingTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalOffboardingTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal offboarding tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays - (double)Quantity, 2);
    }

    public SeasonalOffboardingTier Copy()
    {
        return new SeasonalOffboardingTier(
            Code,
            Label,
            ExternalRef,
            SalaryBand,
            WindowDays,
            Quantity,
            Threshold,
            Reviewer,
            Ratio,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
