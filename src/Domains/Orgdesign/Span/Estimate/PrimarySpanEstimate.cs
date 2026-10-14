namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary span estimate is in its life cycle.</summary>
public enum PrimarySpanEstimateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A primary span estimate. Its code is unique within a tenant.</summary>
public sealed class PrimarySpanEstimate(string code, string label, string costCenter, int retries, string badge, double threshold, string salaryBand, string ownerId, double discount, int updatedAt, bool approved, PrimarySpanEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public double Threshold { get; set; } = threshold;
    public string SalaryBand { get; set; } = salaryBand;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Approved { get; set; } = approved;
    public PrimarySpanEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a primary span estimate in status created.</summary>
    public static PrimarySpanEstimate NewPrimarySpanEstimate(string code, string label)
    {
        return new PrimarySpanEstimate(
            code,
            label,
            "",
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            0,
            false,
            PrimarySpanEstimateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySpanEstimateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySpanEstimateStatus next)
    {
        switch (Status)
        {
            case PrimarySpanEstimateStatus.Created:
                switch (next)
                {
                    case PrimarySpanEstimateStatus.Published:
                    case PrimarySpanEstimateStatus.Retired:
                        return true;
                }
                return false;
            case PrimarySpanEstimateStatus.Validated:
                switch (next)
                {
                    case PrimarySpanEstimateStatus.Published:
                    case PrimarySpanEstimateStatus.Retired:
                        return true;
                }
                return false;
            case PrimarySpanEstimateStatus.Published:
                switch (next)
                {
                    case PrimarySpanEstimateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimarySpanEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary span estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount - (double)UpdatedAt, 2);
    }

    public PrimarySpanEstimate Copy()
    {
        return new PrimarySpanEstimate(
            Code,
            Label,
            CostCenter,
            Retries,
            Badge,
            Threshold,
            SalaryBand,
            OwnerId,
            Discount,
            UpdatedAt,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
