namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback skill profile snapshot is in its life cycle.</summary>
public enum FallbackSkillProfileSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One fallback skill profile snapshot, with what the planners need to act on it.</summary>
public sealed class FallbackSkillProfileSnapshot(string code, string label, string skill, int graceDays, int capacity, bool archived, bool visible, int quantity, List<string> tags, FallbackSkillProfileSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public bool Archived { get; set; } = archived;
    public bool Visible { get; set; } = visible;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public FallbackSkillProfileSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback skill profile snapshot in status draft.</summary>
    public static FallbackSkillProfileSnapshot NewFallbackSkillProfileSnapshot(string code, string label)
    {
        return new FallbackSkillProfileSnapshot(
            code,
            label,
            "",
            0,
            0,
            false,
            false,
            0,
            new List<string>(),
            FallbackSkillProfileSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSkillProfileSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSkillProfileSnapshotStatus next)
    {
        switch (Status)
        {
            case FallbackSkillProfileSnapshotStatus.Draft:
                return next == FallbackSkillProfileSnapshotStatus.Pending || next == FallbackSkillProfileSnapshotStatus.Suspended;
            case FallbackSkillProfileSnapshotStatus.Pending:
                return next == FallbackSkillProfileSnapshotStatus.Closed;
            case FallbackSkillProfileSnapshotStatus.Active:
                return next == FallbackSkillProfileSnapshotStatus.Suspended || next == FallbackSkillProfileSnapshotStatus.Closed;
            case FallbackSkillProfileSnapshotStatus.Suspended:
                switch (next)
                {
                    case FallbackSkillProfileSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSkillProfileSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback skill profile snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double NetLoad()
    {
        return Math.Round((double)Capacity * (double)Quantity, 2);
    }

    public FallbackSkillProfileSnapshot Copy()
    {
        return new FallbackSkillProfileSnapshot(
            Code,
            Label,
            Skill,
            GraceDays,
            Capacity,
            Archived,
            Visible,
            Quantity,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
