namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk preference is in its life cycle.</summary>
public enum DeskPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A desk preference. Its code is unique within a tenant.</summary>
public sealed class DeskPreference(string code, string label, string jobCode, string contactEmail, int createdAt, int capacity, string badge, int version, DeskPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public int Version { get; set; } = version;
    public DeskPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a desk preference in status created.</summary>
    public static DeskPreference NewDeskPreference(string code, string label)
    {
        return new DeskPreference(code, label, "", "", 0, 0, "", 0, DeskPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskPreferenceStatus next)
    {
        switch (Status)
        {
            case DeskPreferenceStatus.Created:
                return next == DeskPreferenceStatus.Validated || next == DeskPreferenceStatus.Retired;
            case DeskPreferenceStatus.Validated:
                switch (next)
                {
                    case DeskPreferenceStatus.Published:
                        return true;
                }
                return false;
            case DeskPreferenceStatus.Published:
                return next == DeskPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DeskPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt - (double)Capacity, 2);
    }

    public DeskPreference Copy()
    {
        return new DeskPreference(Code, Label, JobCode, ContactEmail, CreatedAt, Capacity, Badge, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
