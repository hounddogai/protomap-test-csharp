namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pool tier is in its life cycle.</summary>
public enum ManualPoolTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One manual pool tier, with what the planners need to act on it.</summary>
public sealed class ManualPoolTier(string code, string name, string segment, string description, double maximum, bool visible, ManualPoolTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public bool Visible { get; set; } = visible;
    public ManualPoolTierStatus Status { get; set; } = status;

    /// <summary>Starts a manual pool tier in status proposed.</summary>
    public static ManualPoolTier NewManualPoolTier(string code, string name)
    {
        return new ManualPoolTier(code, name, "", "", 0.0, false, ManualPoolTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPoolTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPoolTierStatus next)
    {
        switch (Status)
        {
            case ManualPoolTierStatus.Proposed:
                switch (next)
                {
                    case ManualPoolTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualPoolTierStatus.Approved:
                switch (next)
                {
                    case ManualPoolTierStatus.Rejected:
                    case ManualPoolTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualPoolTierStatus.Rejected:
                return next == ManualPoolTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualPoolTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pool tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public ManualPoolTier Copy()
    {
        return new ManualPoolTier(Code, Name, Segment, Description, Maximum, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
