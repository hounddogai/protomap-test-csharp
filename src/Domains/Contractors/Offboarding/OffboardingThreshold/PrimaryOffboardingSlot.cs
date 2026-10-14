namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary offboarding slot is in its life cycle.</summary>
public enum PrimaryOffboardingSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary offboarding slot. Its code is unique within a tenant.</summary>
public sealed class PrimaryOffboardingSlot(string code, string name, List<string> tags, string currency, bool archived, double discount, PrimaryOffboardingSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public double Discount { get; set; } = discount;
    public PrimaryOffboardingSlotStatus Status { get; set; } = status;

    /// <summary>Starts a primary offboarding slot in status requested.</summary>
    public static PrimaryOffboardingSlot NewPrimaryOffboardingSlot(string code, string name)
    {
        return new PrimaryOffboardingSlot(
            code,
            name,
            new List<string>(),
            "",
            false,
            0.0,
            PrimaryOffboardingSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryOffboardingSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryOffboardingSlotStatus next)
    {
        switch (Status)
        {
            case PrimaryOffboardingSlotStatus.Requested:
                return next == PrimaryOffboardingSlotStatus.Expired;
            case PrimaryOffboardingSlotStatus.Granted:
                switch (next)
                {
                    case PrimaryOffboardingSlotStatus.Revoked:
                        return true;
                }
                return false;
            case PrimaryOffboardingSlotStatus.Revoked:
                switch (next)
                {
                    case PrimaryOffboardingSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryOffboardingSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary offboarding slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double MeasuredValue()
    {
        return Discount;
    }

    public PrimaryOffboardingSlot Copy()
    {
        return new PrimaryOffboardingSlot(Code, Name, Tags, Currency, Archived, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
