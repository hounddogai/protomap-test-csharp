namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation agreement is in its life cycle.</summary>
public enum AttestationAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One attestation agreement, with what the planners need to act on it.</summary>
public sealed class AttestationAgreement(string code, string label, string category, int capacity, double floorValue, int createdAt, int endsAt, double discount, double weight, AttestationAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public int EndsAt { get; set; } = endsAt;
    public double Discount { get; set; } = discount;
    public double Weight { get; set; } = weight;
    public AttestationAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a attestation agreement in status new.</summary>
    public static AttestationAgreement NewAttestationAgreement(string code, string label)
    {
        return new AttestationAgreement(code, label, "", 0, 0.0, 0, 0, 0.0, 0.0, AttestationAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationAgreementStatus next)
    {
        switch (Status)
        {
            case AttestationAgreementStatus.New:
                switch (next)
                {
                    case AttestationAgreementStatus.Queued:
                    case AttestationAgreementStatus.Done:
                        return true;
                }
                return false;
            case AttestationAgreementStatus.Queued:
                switch (next)
                {
                    case AttestationAgreementStatus.Running:
                    case AttestationAgreementStatus.Failed:
                        return true;
                }
                return false;
            case AttestationAgreementStatus.Running:
                return next == AttestationAgreementStatus.Failed;
            case AttestationAgreementStatus.Done:
                switch (next)
                {
                    case AttestationAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"attestation agreement"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Weight + (double)CreatedAt, 2);
    }

    public AttestationAgreement Copy()
    {
        return new AttestationAgreement(
            Code,
            Label,
            Category,
            Capacity,
            FloorValue,
            CreatedAt,
            EndsAt,
            Discount,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
