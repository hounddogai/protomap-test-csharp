namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile mapping is in its life cycle.</summary>
public enum SkillProfileMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a skill profile mapping and its current status.</summary>
public sealed class SkillProfileMapping(string code, string title, int windowDays, int startsAt, string badge, bool approved, string notes, bool pinned, SkillProfileMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public string Notes { get; set; } = notes;
    public bool Pinned { get; set; } = pinned;
    public SkillProfileMappingStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile mapping in status requested.</summary>
    public static SkillProfileMapping NewSkillProfileMapping(string code, string title)
    {
        return new SkillProfileMapping(code, title, 0, 0, "", false, "", false, SkillProfileMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileMappingStatus next)
    {
        switch (Status)
        {
            case SkillProfileMappingStatus.Requested:
                return next == SkillProfileMappingStatus.Granted;
            case SkillProfileMappingStatus.Granted:
                return next == SkillProfileMappingStatus.Revoked;
            case SkillProfileMappingStatus.Revoked:
                return next == SkillProfileMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SkillProfileMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt + (double)WindowDays, 2);
    }

    public SkillProfileMapping Copy()
    {
        return new SkillProfileMapping(Code, Title, WindowDays, StartsAt, Badge, Approved, Notes, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
