namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive skill profile snapshot is in its life cycle.</summary>
public enum AdaptiveSkillProfileSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive skill profile snapshot as the skill profile team keeps it.</summary>
public sealed class AdaptiveSkillProfileSnapshot(string code, string label, string currency, int windowDays, string employeeId, AdaptiveSkillProfileSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public string EmployeeId { get; set; } = employeeId;
    public AdaptiveSkillProfileSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive skill profile snapshot in status requested.</summary>
    public static AdaptiveSkillProfileSnapshot NewAdaptiveSkillProfileSnapshot(string code, string label)
    {
        return new AdaptiveSkillProfileSnapshot(
            code,
            label,
            "",
            0,
            "",
            AdaptiveSkillProfileSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveSkillProfileSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveSkillProfileSnapshotStatus next)
    {
        switch (Status)
        {
            case AdaptiveSkillProfileSnapshotStatus.Requested:
                return next == AdaptiveSkillProfileSnapshotStatus.Expired;
            case AdaptiveSkillProfileSnapshotStatus.Granted:
                return next == AdaptiveSkillProfileSnapshotStatus.Revoked || next == AdaptiveSkillProfileSnapshotStatus.Expired;
            case AdaptiveSkillProfileSnapshotStatus.Revoked:
                switch (next)
                {
                    case AdaptiveSkillProfileSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveSkillProfileSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive skill profile snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public AdaptiveSkillProfileSnapshot Copy()
    {
        return new AdaptiveSkillProfileSnapshot(Code, Label, Currency, WindowDays, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
