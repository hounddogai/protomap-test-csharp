namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered skill profile proposal is in its life cycle.</summary>
public enum TieredSkillProfileProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered skill profile proposal. Its code is unique within a tenant.</summary>
public sealed class TieredSkillProfileProposal(string code, string label, string region, string grade, int windowDays, int version, string notes, TieredSkillProfileProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public string Grade { get; set; } = grade;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public TieredSkillProfileProposalStatus Status { get; set; } = status;

    /// <summary>Starts a tiered skill profile proposal in status requested.</summary>
    public static TieredSkillProfileProposal NewTieredSkillProfileProposal(string code, string label)
    {
        return new TieredSkillProfileProposal(
            code,
            label,
            "",
            "",
            0,
            0,
            "",
            TieredSkillProfileProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSkillProfileProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSkillProfileProposalStatus next)
    {
        switch (Status)
        {
            case TieredSkillProfileProposalStatus.Requested:
                switch (next)
                {
                    case TieredSkillProfileProposalStatus.Revoked:
                        return true;
                }
                return false;
            case TieredSkillProfileProposalStatus.Granted:
                return next == TieredSkillProfileProposalStatus.Revoked;
            case TieredSkillProfileProposalStatus.Revoked:
                return next == TieredSkillProfileProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredSkillProfileProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered skill profile proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"tiered skill profile proposal"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays * (double)Version, 2);
    }

    public TieredSkillProfileProposal Copy()
    {
        return new TieredSkillProfileProposal(Code, Label, Region, Grade, WindowDays, Version, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
