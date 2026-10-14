namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary attestation entry is in its life cycle.</summary>
public enum PrimaryAttestationEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary attestation entry as the attestation team keeps it.</summary>
public sealed class PrimaryAttestationEntry(string code, string label, int tenureMonths, double fte, string reviewer, Dictionary<string, string> attributes, string category, int retries, int capacity, int createdAt, PrimaryAttestationEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Fte { get; set; } = fte;
    public string Reviewer { get; set; } = reviewer;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public int Retries { get; set; } = retries;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public PrimaryAttestationEntryStatus Status { get; set; } = status;

    /// <summary>Starts a primary attestation entry in status planned.</summary>
    public static PrimaryAttestationEntry NewPrimaryAttestationEntry(string code, string label)
    {
        return new PrimaryAttestationEntry(
            code,
            label,
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0,
            0,
            PrimaryAttestationEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAttestationEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAttestationEntryStatus next)
    {
        switch (Status)
        {
            case PrimaryAttestationEntryStatus.Planned:
                return next == PrimaryAttestationEntryStatus.InProgress || next == PrimaryAttestationEntryStatus.Cancelled;
            case PrimaryAttestationEntryStatus.Scheduled:
                return next == PrimaryAttestationEntryStatus.Completed;
            case PrimaryAttestationEntryStatus.InProgress:
                switch (next)
                {
                    case PrimaryAttestationEntryStatus.Completed:
                    case PrimaryAttestationEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryAttestationEntryStatus.Completed:
                switch (next)
                {
                    case PrimaryAttestationEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAttestationEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary attestation entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetValue()
    {
        return Math.Round((double)CreatedAt * (double)Retries, 2);
    }

    public PrimaryAttestationEntry Copy()
    {
        return new PrimaryAttestationEntry(
            Code,
            Label,
            TenureMonths,
            Fte,
            Reviewer,
            Attributes,
            Category,
            Retries,
            Capacity,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
