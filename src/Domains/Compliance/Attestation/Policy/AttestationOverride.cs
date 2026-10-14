namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation override is in its life cycle.</summary>
public enum AttestationOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a attestation override and its current status.</summary>
public sealed class AttestationOverride(string code, string label, int graceDays, int batchSize, double threshold, bool visible, AttestationOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int BatchSize { get; set; } = batchSize;
    public double Threshold { get; set; } = threshold;
    public bool Visible { get; set; } = visible;
    public AttestationOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a attestation override in status created.</summary>
    public static AttestationOverride NewAttestationOverride(string code, string label)
    {
        return new AttestationOverride(code, label, 0, 0, 0.0, false, AttestationOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationOverrideStatus next)
    {
        switch (Status)
        {
            case AttestationOverrideStatus.Created:
                switch (next)
                {
                    case AttestationOverrideStatus.Retired:
                        return true;
                }
                return false;
            case AttestationOverrideStatus.Validated:
                switch (next)
                {
                    case AttestationOverrideStatus.Published:
                    case AttestationOverrideStatus.Retired:
                        return true;
                }
                return false;
            case AttestationOverrideStatus.Published:
                return next == AttestationOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AttestationOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)BatchSize * Threshold, 2);
    }

    public AttestationOverride Copy()
    {
        return new AttestationOverride(Code, Label, GraceDays, BatchSize, Threshold, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
