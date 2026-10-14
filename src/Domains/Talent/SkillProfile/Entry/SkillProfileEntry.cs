namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile entry is in its life cycle.</summary>
public enum SkillProfileEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A skill profile entry. Its code is unique within a tenant.</summary>
public sealed class SkillProfileEntry(string code, string label, int limit, bool archived, int capacity, SkillProfileEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public bool Archived { get; set; } = archived;
    public int Capacity { get; set; } = capacity;
    public SkillProfileEntryStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile entry in status proposed.</summary>
    public static SkillProfileEntry NewSkillProfileEntry(string code, string label)
    {
        return new SkillProfileEntry(code, label, 0, false, 0, SkillProfileEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileEntryStatus next)
    {
        switch (Status)
        {
            case SkillProfileEntryStatus.Proposed:
                return next == SkillProfileEntryStatus.Withdrawn;
            case SkillProfileEntryStatus.Approved:
                return next == SkillProfileEntryStatus.Rejected || next == SkillProfileEntryStatus.Withdrawn;
            case SkillProfileEntryStatus.Rejected:
                return next == SkillProfileEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SkillProfileEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)Capacity + (double)Limit, 2);
    }

    public SkillProfileEntry Copy()
    {
        return new SkillProfileEntry(Code, Label, Limit, Archived, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
