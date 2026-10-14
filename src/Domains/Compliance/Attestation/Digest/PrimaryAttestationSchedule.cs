namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary attestation schedule is in its life cycle.</summary>
public enum PrimaryAttestationScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary attestation schedule. Its code is unique within a tenant.</summary>
public sealed class PrimaryAttestationSchedule(string code, string name, int updatedAt, string locale, string timezone, string jobCode, bool pinned, int version, string label, List<string> tags, PrimaryAttestationScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public PrimaryAttestationScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a primary attestation schedule in status proposed.</summary>
    public static PrimaryAttestationSchedule NewPrimaryAttestationSchedule(string code, string name)
    {
        return new PrimaryAttestationSchedule(
            code,
            name,
            0,
            "",
            "",
            "",
            false,
            0,
            "",
            new List<string>(),
            PrimaryAttestationScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAttestationScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAttestationScheduleStatus next)
    {
        switch (Status)
        {
            case PrimaryAttestationScheduleStatus.Proposed:
                return next == PrimaryAttestationScheduleStatus.Rejected;
            case PrimaryAttestationScheduleStatus.Approved:
                return next == PrimaryAttestationScheduleStatus.Rejected || next == PrimaryAttestationScheduleStatus.Withdrawn;
            case PrimaryAttestationScheduleStatus.Rejected:
                switch (next)
                {
                    case PrimaryAttestationScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAttestationScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary attestation schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"primary attestation schedule"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version - (double)UpdatedAt, 2);
    }

    public PrimaryAttestationSchedule Copy()
    {
        return new PrimaryAttestationSchedule(
            Code,
            Name,
            UpdatedAt,
            Locale,
            Timezone,
            JobCode,
            Pinned,
            Version,
            Label,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
