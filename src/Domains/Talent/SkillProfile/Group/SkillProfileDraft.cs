namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile draft is in its life cycle.</summary>
public enum SkillProfileDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One skill profile draft, with what the planners need to act on it.</summary>
public sealed class SkillProfileDraft(string code, string name, int version, int sequence, bool visible, List<string> tags, string source, int retries, string country, int endsAt, string phone, SkillProfileDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public int Sequence { get; set; } = sequence;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public string Country { get; set; } = country;
    public int EndsAt { get; set; } = endsAt;
    public string Phone { get; set; } = phone;
    public SkillProfileDraftStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile draft in status proposed.</summary>
    public static SkillProfileDraft NewSkillProfileDraft(string code, string name)
    {
        return new SkillProfileDraft(
            code,
            name,
            0,
            0,
            false,
            new List<string>(),
            "",
            0,
            "",
            0,
            "",
            SkillProfileDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileDraftStatus next)
    {
        switch (Status)
        {
            case SkillProfileDraftStatus.Proposed:
                switch (next)
                {
                    case SkillProfileDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case SkillProfileDraftStatus.Approved:
                switch (next)
                {
                    case SkillProfileDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case SkillProfileDraftStatus.Rejected:
                return next == SkillProfileDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SkillProfileDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt - (double)Version, 2);
    }

    public SkillProfileDraft Copy()
    {
        return new SkillProfileDraft(
            Code,
            Name,
            Version,
            Sequence,
            Visible,
            Tags,
            Source,
            Retries,
            Country,
            EndsAt,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
