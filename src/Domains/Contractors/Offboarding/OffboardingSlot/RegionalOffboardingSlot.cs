namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional offboarding slot is in its life cycle.</summary>
public enum RegionalOffboardingSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional offboarding slot, with what the planners need to act on it.</summary>
public sealed class RegionalOffboardingSlot(string code, string name, string department, string contactEmail, double maximum, string externalRef, string badge, int batchSize, double ceiling, string managerId, string costCenter, RegionalOffboardingSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string ContactEmail { get; set; } = contactEmail;
    public double Maximum { get; set; } = maximum;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public double Ceiling { get; set; } = ceiling;
    public string ManagerId { get; set; } = managerId;
    public string CostCenter { get; set; } = costCenter;
    public RegionalOffboardingSlotStatus Status { get; set; } = status;

    /// <summary>Starts a regional offboarding slot in status proposed.</summary>
    public static RegionalOffboardingSlot NewRegionalOffboardingSlot(string code, string name)
    {
        return new RegionalOffboardingSlot(
            code,
            name,
            "",
            "",
            0.0,
            "",
            "",
            0,
            0.0,
            "",
            "",
            RegionalOffboardingSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalOffboardingSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalOffboardingSlotStatus next)
    {
        switch (Status)
        {
            case RegionalOffboardingSlotStatus.Proposed:
                switch (next)
                {
                    case RegionalOffboardingSlotStatus.Approved:
                        return true;
                }
                return false;
            case RegionalOffboardingSlotStatus.Approved:
                switch (next)
                {
                    case RegionalOffboardingSlotStatus.Rejected:
                        return true;
                }
                return false;
            case RegionalOffboardingSlotStatus.Rejected:
                return next == RegionalOffboardingSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalOffboardingSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional offboarding slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum + Ceiling, 2);
    }

    public RegionalOffboardingSlot Copy()
    {
        return new RegionalOffboardingSlot(
            Code,
            Name,
            Department,
            ContactEmail,
            Maximum,
            ExternalRef,
            Badge,
            BatchSize,
            Ceiling,
            ManagerId,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
