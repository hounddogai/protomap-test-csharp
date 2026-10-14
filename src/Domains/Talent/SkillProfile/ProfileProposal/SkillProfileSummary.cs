namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile summary is in its life cycle.</summary>
public enum SkillProfileSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One skill profile summary, with what the planners need to act on it.</summary>
public sealed class SkillProfileSummary(string code, string name, string skill, string label, string checksum, string notes, Dictionary<string, string> attributes, string externalRef, double rate, string team, int version, SkillProfileSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ExternalRef { get; set; } = externalRef;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public int Version { get; set; } = version;
    public SkillProfileSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile summary in status proposed.</summary>
    public static SkillProfileSummary NewSkillProfileSummary(string code, string name)
    {
        return new SkillProfileSummary(
            code,
            name,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            0,
            SkillProfileSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileSummaryStatus next)
    {
        switch (Status)
        {
            case SkillProfileSummaryStatus.Proposed:
                switch (next)
                {
                    case SkillProfileSummaryStatus.Approved:
                    case SkillProfileSummaryStatus.Rejected:
                        return true;
                }
                return false;
            case SkillProfileSummaryStatus.Approved:
                return next == SkillProfileSummaryStatus.Withdrawn;
            case SkillProfileSummaryStatus.Rejected:
                switch (next)
                {
                    case SkillProfileSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedExposure()
    {
        return Math.Round(Rate * (double)Version, 2);
    }

    public SkillProfileSummary Copy()
    {
        return new SkillProfileSummary(
            Code,
            Name,
            Skill,
            Label,
            Checksum,
            Notes,
            Attributes,
            ExternalRef,
            Rate,
            Team,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
