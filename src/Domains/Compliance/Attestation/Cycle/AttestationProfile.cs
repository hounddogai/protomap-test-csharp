namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation profile is in its life cycle.</summary>
public enum AttestationProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A attestation profile. Its code is unique within a tenant.</summary>
public sealed class AttestationProfile(string code, string label, string country, int quantity, bool archived, string currency, string summary, AttestationProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int Quantity { get; set; } = quantity;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public AttestationProfileStatus Status { get; set; } = status;

    /// <summary>Starts a attestation profile in status new.</summary>
    public static AttestationProfile NewAttestationProfile(string code, string label)
    {
        return new AttestationProfile(code, label, "", 0, false, "", "", AttestationProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationProfileStatus next)
    {
        switch (Status)
        {
            case AttestationProfileStatus.New:
                switch (next)
                {
                    case AttestationProfileStatus.Queued:
                    case AttestationProfileStatus.Running:
                        return true;
                }
                return false;
            case AttestationProfileStatus.Queued:
                return next == AttestationProfileStatus.Failed;
            case AttestationProfileStatus.Running:
                switch (next)
                {
                    case AttestationProfileStatus.Done:
                    case AttestationProfileStatus.Failed:
                        return true;
                }
                return false;
            case AttestationProfileStatus.Done:
                return next == AttestationProfileStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AttestationProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"attestation profile"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AttestationProfile Copy()
    {
        return new AttestationProfile(Code, Label, Country, Quantity, Archived, Currency, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
