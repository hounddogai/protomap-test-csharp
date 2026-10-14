namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped attestation quota is in its life cycle.</summary>
public enum CappedAttestationQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a capped attestation quota and its current status.</summary>
public sealed class CappedAttestationQuota(string code, string label, string segment, string salaryBand, int quantity, int updatedAt, double threshold, int startsAt, string skill, int leaveDays, string region, CappedAttestationQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string SalaryBand { get; set; } = salaryBand;
    public int Quantity { get; set; } = quantity;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Threshold { get; set; } = threshold;
    public int StartsAt { get; set; } = startsAt;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public CappedAttestationQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a capped attestation quota in status created.</summary>
    public static CappedAttestationQuota NewCappedAttestationQuota(string code, string label)
    {
        return new CappedAttestationQuota(
            code,
            label,
            "",
            "",
            0,
            0,
            0.0,
            0,
            "",
            0,
            "",
            CappedAttestationQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAttestationQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAttestationQuotaStatus next)
    {
        switch (Status)
        {
            case CappedAttestationQuotaStatus.Created:
                switch (next)
                {
                    case CappedAttestationQuotaStatus.Published:
                    case CappedAttestationQuotaStatus.Retired:
                        return true;
                }
                return false;
            case CappedAttestationQuotaStatus.Validated:
                switch (next)
                {
                    case CappedAttestationQuotaStatus.Published:
                    case CappedAttestationQuotaStatus.Retired:
                        return true;
                }
                return false;
            case CappedAttestationQuotaStatus.Published:
                switch (next)
                {
                    case CappedAttestationQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAttestationQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped attestation quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped attestation quota"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Threshold + (double)Quantity, 2);
    }

    public CappedAttestationQuota Copy()
    {
        return new CappedAttestationQuota(
            Code,
            Label,
            Segment,
            SalaryBand,
            Quantity,
            UpdatedAt,
            Threshold,
            StartsAt,
            Skill,
            LeaveDays,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
