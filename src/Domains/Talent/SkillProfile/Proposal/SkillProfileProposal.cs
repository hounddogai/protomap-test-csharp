namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile proposal is in its life cycle.</summary>
public enum SkillProfileProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A skill profile proposal as the skill profile team keeps it.</summary>
public sealed class SkillProfileProposal(string code, string label, string category, bool pinned, Dictionary<string, string> attributes, int windowDays, int version, int startsAt, SkillProfileProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public SkillProfileProposalStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile proposal in status requested.</summary>
    public static SkillProfileProposal NewSkillProfileProposal(string code, string label)
    {
        return new SkillProfileProposal(
            code,
            label,
            "",
            false,
            new Dictionary<string, string>(),
            0,
            0,
            0,
            SkillProfileProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileProposalStatus next)
    {
        switch (Status)
        {
            case SkillProfileProposalStatus.Requested:
                return next == SkillProfileProposalStatus.Expired;
            case SkillProfileProposalStatus.Granted:
                return next == SkillProfileProposalStatus.Revoked || next == SkillProfileProposalStatus.Expired;
            case SkillProfileProposalStatus.Revoked:
                return next == SkillProfileProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SkillProfileProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double NetExposure()
    {
        return Math.Round((double)StartsAt + (double)WindowDays, 2);
    }

    public SkillProfileProposal Copy()
    {
        return new SkillProfileProposal(
            Code,
            Label,
            Category,
            Pinned,
            Attributes,
            WindowDays,
            Version,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
