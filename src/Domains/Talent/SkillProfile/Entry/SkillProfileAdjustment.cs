namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile adjustment is in its life cycle.</summary>
public enum SkillProfileAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One skill profile adjustment, with what the planners need to act on it.</summary>
public sealed class SkillProfileAdjustment(string code, string label, string badge, int windowDays, string externalRef, int limit, string checksum, string description, SkillProfileAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int WindowDays { get; set; } = windowDays;
    public string ExternalRef { get; set; } = externalRef;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public string Description { get; set; } = description;
    public SkillProfileAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile adjustment in status idle.</summary>
    public static SkillProfileAdjustment NewSkillProfileAdjustment(string code, string label)
    {
        return new SkillProfileAdjustment(code, label, "", 0, "", 0, "", "", SkillProfileAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileAdjustmentStatus next)
    {
        switch (Status)
        {
            case SkillProfileAdjustmentStatus.Idle:
                return next == SkillProfileAdjustmentStatus.Warming || next == SkillProfileAdjustmentStatus.Stopped;
            case SkillProfileAdjustmentStatus.Warming:
                return next == SkillProfileAdjustmentStatus.Ready || next == SkillProfileAdjustmentStatus.Draining;
            case SkillProfileAdjustmentStatus.Ready:
                return next == SkillProfileAdjustmentStatus.Stopped;
            case SkillProfileAdjustmentStatus.Draining:
                switch (next)
                {
                    case SkillProfileAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit + (double)WindowDays, 2);
    }

    public SkillProfileAdjustment Copy()
    {
        return new SkillProfileAdjustment(
            Code,
            Label,
            Badge,
            WindowDays,
            ExternalRef,
            Limit,
            Checksum,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
