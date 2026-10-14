namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation entry is in its life cycle.</summary>
public enum AttestationEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a attestation entry and its current status.</summary>
public sealed class AttestationEntry(string code, string label, string source, double fte, string currency, string title, int tenureMonths, int updatedAt, AttestationEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Fte { get; set; } = fte;
    public string Currency { get; set; } = currency;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public int UpdatedAt { get; set; } = updatedAt;
    public AttestationEntryStatus Status { get; set; } = status;

    /// <summary>Starts a attestation entry in status proposed.</summary>
    public static AttestationEntry NewAttestationEntry(string code, string label)
    {
        return new AttestationEntry(code, label, "", 0.0, "", "", 0, 0, AttestationEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationEntryStatus next)
    {
        switch (Status)
        {
            case AttestationEntryStatus.Proposed:
                return next == AttestationEntryStatus.Rejected || next == AttestationEntryStatus.Withdrawn;
            case AttestationEntryStatus.Approved:
                return next == AttestationEntryStatus.Rejected;
            case AttestationEntryStatus.Rejected:
                return next == AttestationEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AttestationEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"attestation entry"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Fte - (double)UpdatedAt, 2);
    }

    public AttestationEntry Copy()
    {
        return new AttestationEntry(Code, Label, Source, Fte, Currency, Title, TenureMonths, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
