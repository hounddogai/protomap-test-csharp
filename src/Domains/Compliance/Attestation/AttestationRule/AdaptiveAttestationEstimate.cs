namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive attestation estimate is in its life cycle.</summary>
public enum AdaptiveAttestationEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive attestation estimate as the attestation team keeps it.</summary>
public sealed class AdaptiveAttestationEstimate(string code, string title, double threshold, int tenureMonths, double rating, string jobCode, string unit, double hours, bool visible, string managerId, AdaptiveAttestationEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Rating { get; set; } = rating;
    public string JobCode { get; set; } = jobCode;
    public string Unit { get; set; } = unit;
    public double Hours { get; set; } = hours;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public AdaptiveAttestationEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive attestation estimate in status proposed.</summary>
    public static AdaptiveAttestationEstimate NewAdaptiveAttestationEstimate(string code, string title)
    {
        return new AdaptiveAttestationEstimate(
            code,
            title,
            0.0,
            0,
            0.0,
            "",
            "",
            0.0,
            false,
            "",
            AdaptiveAttestationEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAttestationEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAttestationEstimateStatus next)
    {
        switch (Status)
        {
            case AdaptiveAttestationEstimateStatus.Proposed:
                switch (next)
                {
                    case AdaptiveAttestationEstimateStatus.Approved:
                        return true;
                }
                return false;
            case AdaptiveAttestationEstimateStatus.Approved:
                return next == AdaptiveAttestationEstimateStatus.Rejected;
            case AdaptiveAttestationEstimateStatus.Rejected:
                return next == AdaptiveAttestationEstimateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveAttestationEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive attestation estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Threshold - Hours, 2);
    }

    public AdaptiveAttestationEstimate Copy()
    {
        return new AdaptiveAttestationEstimate(
            Code,
            Title,
            Threshold,
            TenureMonths,
            Rating,
            JobCode,
            Unit,
            Hours,
            Visible,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
