namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation estimate is in its life cycle.</summary>
public enum AttestationEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A attestation estimate as the attestation team keeps it.</summary>
public sealed class AttestationEstimate(string code, string name, double discount, int retries, bool approved, string contactEmail, double margin, string currency, int expiresAt, double floorValue, double maximum, AttestationEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public string ContactEmail { get; set; } = contactEmail;
    public double Margin { get; set; } = margin;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public AttestationEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a attestation estimate in status draft.</summary>
    public static AttestationEstimate NewAttestationEstimate(string code, string name)
    {
        return new AttestationEstimate(
            code,
            name,
            0.0,
            0,
            false,
            "",
            0.0,
            "",
            0,
            0.0,
            0.0,
            AttestationEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationEstimateStatus next)
    {
        switch (Status)
        {
            case AttestationEstimateStatus.Draft:
                return next == AttestationEstimateStatus.Pending;
            case AttestationEstimateStatus.Pending:
                return next == AttestationEstimateStatus.Closed;
            case AttestationEstimateStatus.Active:
                switch (next)
                {
                    case AttestationEstimateStatus.Closed:
                        return true;
                }
                return false;
            case AttestationEstimateStatus.Suspended:
                return next == AttestationEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AttestationEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"attestation estimate"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Discount * (double)Retries, 2);
    }

    public AttestationEstimate Copy()
    {
        return new AttestationEstimate(
            Code,
            Name,
            Discount,
            Retries,
            Approved,
            ContactEmail,
            Margin,
            Currency,
            ExpiresAt,
            FloorValue,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
