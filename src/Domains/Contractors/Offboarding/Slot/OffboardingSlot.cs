namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding slot is in its life cycle.</summary>
public enum OffboardingSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a offboarding slot and its current status.</summary>
public sealed class OffboardingSlot(string code, string name, string label, int retries, int leaveDays, int version, double fte, string managerId, OffboardingSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int LeaveDays { get; set; } = leaveDays;
    public int Version { get; set; } = version;
    public double Fte { get; set; } = fte;
    public string ManagerId { get; set; } = managerId;
    public OffboardingSlotStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding slot in status created.</summary>
    public static OffboardingSlot NewOffboardingSlot(string code, string name)
    {
        return new OffboardingSlot(code, name, "", 0, 0, 0, 0.0, "", OffboardingSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingSlotStatus next)
    {
        switch (Status)
        {
            case OffboardingSlotStatus.Created:
                switch (next)
                {
                    case OffboardingSlotStatus.Validated:
                        return true;
                }
                return false;
            case OffboardingSlotStatus.Validated:
                return next == OffboardingSlotStatus.Published;
            case OffboardingSlotStatus.Published:
                return next == OffboardingSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(OffboardingSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Version + Fte, 2);
    }

    public OffboardingSlot Copy()
    {
        return new OffboardingSlot(Code, Name, Label, Retries, LeaveDays, Version, Fte, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
