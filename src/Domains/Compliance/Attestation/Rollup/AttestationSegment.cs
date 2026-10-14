namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation segment is in its life cycle.</summary>
public enum AttestationSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a attestation segment and its current status.</summary>
public sealed class AttestationSegment(string code, string name, bool approved, string label, double ceiling, string summary, string location, string costCenter, double weight, string salaryBand, AttestationSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public double Weight { get; set; } = weight;
    public string SalaryBand { get; set; } = salaryBand;
    public AttestationSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a attestation segment in status requested.</summary>
    public static AttestationSegment NewAttestationSegment(string code, string name)
    {
        return new AttestationSegment(
            code,
            name,
            false,
            "",
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            AttestationSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationSegmentStatus next)
    {
        switch (Status)
        {
            case AttestationSegmentStatus.Requested:
                return next == AttestationSegmentStatus.Granted;
            case AttestationSegmentStatus.Granted:
                return next == AttestationSegmentStatus.Revoked;
            case AttestationSegmentStatus.Revoked:
                return next == AttestationSegmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AttestationSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"attestation segment"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Ceiling - Weight, 2);
    }

    public AttestationSegment Copy()
    {
        return new AttestationSegment(
            Code,
            Name,
            Approved,
            Label,
            Ceiling,
            Summary,
            Location,
            CostCenter,
            Weight,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
