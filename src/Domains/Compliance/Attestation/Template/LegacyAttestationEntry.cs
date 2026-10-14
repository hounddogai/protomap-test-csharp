namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy attestation entry is in its life cycle.</summary>
public enum LegacyAttestationEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a legacy attestation entry and its current status.</summary>
public sealed class LegacyAttestationEntry(string code, string label, double hours, string timezone, double ratio, string jobCode, string region, LegacyAttestationEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Timezone { get; set; } = timezone;
    public double Ratio { get; set; } = ratio;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public LegacyAttestationEntryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy attestation entry in status planned.</summary>
    public static LegacyAttestationEntry NewLegacyAttestationEntry(string code, string label)
    {
        return new LegacyAttestationEntry(code, label, 0.0, "", 0.0, "", "", LegacyAttestationEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAttestationEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAttestationEntryStatus next)
    {
        switch (Status)
        {
            case LegacyAttestationEntryStatus.Planned:
                return next == LegacyAttestationEntryStatus.InProgress || next == LegacyAttestationEntryStatus.Cancelled;
            case LegacyAttestationEntryStatus.Scheduled:
                switch (next)
                {
                    case LegacyAttestationEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyAttestationEntryStatus.InProgress:
                return next == LegacyAttestationEntryStatus.Completed || next == LegacyAttestationEntryStatus.Cancelled;
            case LegacyAttestationEntryStatus.Completed:
                return next == LegacyAttestationEntryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacyAttestationEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy attestation entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ratio - Hours, 2);
    }

    public LegacyAttestationEntry Copy()
    {
        return new LegacyAttestationEntry(Code, Label, Hours, Timezone, Ratio, JobCode, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
