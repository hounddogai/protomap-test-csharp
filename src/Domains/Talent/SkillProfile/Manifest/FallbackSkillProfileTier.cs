namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback skill profile tier is in its life cycle.</summary>
public enum FallbackSkillProfileTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback skill profile tier. Its code is unique within a tenant.</summary>
public sealed class FallbackSkillProfileTier(string code, string name, bool pinned, int version, List<string> tags, string grade, double ceiling, FallbackSkillProfileTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public List<string> Tags { get; set; } = tags;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public FallbackSkillProfileTierStatus Status { get; set; } = status;

    /// <summary>Starts a fallback skill profile tier in status new.</summary>
    public static FallbackSkillProfileTier NewFallbackSkillProfileTier(string code, string name)
    {
        return new FallbackSkillProfileTier(
            code,
            name,
            false,
            0,
            new List<string>(),
            "",
            0.0,
            FallbackSkillProfileTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSkillProfileTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSkillProfileTierStatus next)
    {
        switch (Status)
        {
            case FallbackSkillProfileTierStatus.New:
                return next == FallbackSkillProfileTierStatus.Queued || next == FallbackSkillProfileTierStatus.Failed;
            case FallbackSkillProfileTierStatus.Queued:
                switch (next)
                {
                    case FallbackSkillProfileTierStatus.Running:
                    case FallbackSkillProfileTierStatus.Done:
                        return true;
                }
                return false;
            case FallbackSkillProfileTierStatus.Running:
                switch (next)
                {
                    case FallbackSkillProfileTierStatus.Done:
                        return true;
                }
                return false;
            case FallbackSkillProfileTierStatus.Done:
                switch (next)
                {
                    case FallbackSkillProfileTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSkillProfileTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback skill profile tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)Version + Ceiling, 2);
    }

    public FallbackSkillProfileTier Copy()
    {
        return new FallbackSkillProfileTier(Code, Name, Pinned, Version, Tags, Grade, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
