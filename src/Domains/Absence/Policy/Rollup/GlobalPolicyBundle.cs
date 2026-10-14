namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global policy bundle is in its life cycle.</summary>
public enum GlobalPolicyBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global policy bundle. Its code is unique within a tenant.</summary>
public sealed class GlobalPolicyBundle(string code, string label, int startsAt, string unit, string description, bool visible, int revision, int sequence, GlobalPolicyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public int Revision { get; set; } = revision;
    public int Sequence { get; set; } = sequence;
    public GlobalPolicyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global policy bundle in status draft.</summary>
    public static GlobalPolicyBundle NewGlobalPolicyBundle(string code, string label)
    {
        return new GlobalPolicyBundle(code, label, 0, "", "", false, 0, 0, GlobalPolicyBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPolicyBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPolicyBundleStatus next)
    {
        switch (Status)
        {
            case GlobalPolicyBundleStatus.Draft:
                return next == GlobalPolicyBundleStatus.Pending || next == GlobalPolicyBundleStatus.Closed;
            case GlobalPolicyBundleStatus.Pending:
                switch (next)
                {
                    case GlobalPolicyBundleStatus.Suspended:
                        return true;
                }
                return false;
            case GlobalPolicyBundleStatus.Active:
                switch (next)
                {
                    case GlobalPolicyBundleStatus.Suspended:
                    case GlobalPolicyBundleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalPolicyBundleStatus.Suspended:
                return next == GlobalPolicyBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalPolicyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global policy bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence + (double)Revision, 2);
    }

    public GlobalPolicyBundle Copy()
    {
        return new GlobalPolicyBundle(Code, Label, StartsAt, Unit, Description, Visible, Revision, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
