namespace Acme.Workforce.Compliance.Attestation.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal attestation mapping is in its life cycle.</summary>
public enum SeasonalAttestationMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal attestation mapping as the attestation team keeps it.</summary>
public sealed class SeasonalAttestationMapping(string code, string name, int updatedAt, string costCenter, string externalRef, double discount, string employeeId, string phone, string segment, double fte, SeasonalAttestationMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public SeasonalAttestationMappingStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal attestation mapping in status planned.</summary>
    public static SeasonalAttestationMapping NewSeasonalAttestationMapping(string code, string name)
    {
        return new SeasonalAttestationMapping(
            code,
            name,
            0,
            "",
            "",
            0.0,
            "",
            "",
            "",
            0.0,
            SeasonalAttestationMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAttestationMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAttestationMappingStatus next)
    {
        switch (Status)
        {
            case SeasonalAttestationMappingStatus.Planned:
                return next == SeasonalAttestationMappingStatus.Completed;
            case SeasonalAttestationMappingStatus.Scheduled:
                return next == SeasonalAttestationMappingStatus.InProgress || next == SeasonalAttestationMappingStatus.Cancelled;
            case SeasonalAttestationMappingStatus.InProgress:
                return next == SeasonalAttestationMappingStatus.Completed || next == SeasonalAttestationMappingStatus.Cancelled;
            case SeasonalAttestationMappingStatus.Completed:
                switch (next)
                {
                    case SeasonalAttestationMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAttestationMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal attestation mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)UpdatedAt - Fte, 2);
    }

    public SeasonalAttestationMapping Copy()
    {
        return new SeasonalAttestationMapping(
            Code,
            Name,
            UpdatedAt,
            CostCenter,
            ExternalRef,
            Discount,
            EmployeeId,
            Phone,
            Segment,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
