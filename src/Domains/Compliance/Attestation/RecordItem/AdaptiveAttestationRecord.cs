namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive attestation record is in its life cycle.</summary>
public enum AdaptiveAttestationRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive attestation record. Its code is unique within a tenant.</summary>
public sealed class AdaptiveAttestationRecord(string code, string label, string timezone, bool archived, double rating, AdaptiveAttestationRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public bool Archived { get; set; } = archived;
    public double Rating { get; set; } = rating;
    public AdaptiveAttestationRecordStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive attestation record in status draft.</summary>
    public static AdaptiveAttestationRecord NewAdaptiveAttestationRecord(string code, string label)
    {
        return new AdaptiveAttestationRecord(code, label, "", false, 0.0, AdaptiveAttestationRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAttestationRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAttestationRecordStatus next)
    {
        switch (Status)
        {
            case AdaptiveAttestationRecordStatus.Draft:
                switch (next)
                {
                    case AdaptiveAttestationRecordStatus.Active:
                        return true;
                }
                return false;
            case AdaptiveAttestationRecordStatus.Pending:
                switch (next)
                {
                    case AdaptiveAttestationRecordStatus.Active:
                        return true;
                }
                return false;
            case AdaptiveAttestationRecordStatus.Active:
                return next == AdaptiveAttestationRecordStatus.Suspended;
            case AdaptiveAttestationRecordStatus.Suspended:
                switch (next)
                {
                    case AdaptiveAttestationRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveAttestationRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive attestation record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public AdaptiveAttestationRecord Copy()
    {
        return new AdaptiveAttestationRecord(Code, Label, Timezone, Archived, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
