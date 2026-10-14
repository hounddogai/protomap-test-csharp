namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional asset return proposal is in its life cycle.</summary>
public enum RegionalAssetReturnProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional asset return proposal as the asset return team keeps it.</summary>
public sealed class RegionalAssetReturnProposal(string code, string title, string skill, double threshold, double ratio, List<string> tags, int sequence, int windowDays, string phone, RegionalAssetReturnProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public double Threshold { get; set; } = threshold;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public string Phone { get; set; } = phone;
    public RegionalAssetReturnProposalStatus Status { get; set; } = status;

    /// <summary>Starts a regional asset return proposal in status draft.</summary>
    public static RegionalAssetReturnProposal NewRegionalAssetReturnProposal(string code, string title)
    {
        return new RegionalAssetReturnProposal(
            code,
            title,
            "",
            0.0,
            0.0,
            new List<string>(),
            0,
            0,
            "",
            RegionalAssetReturnProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssetReturnProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssetReturnProposalStatus next)
    {
        switch (Status)
        {
            case RegionalAssetReturnProposalStatus.Draft:
                switch (next)
                {
                    case RegionalAssetReturnProposalStatus.Pending:
                    case RegionalAssetReturnProposalStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalAssetReturnProposalStatus.Pending:
                switch (next)
                {
                    case RegionalAssetReturnProposalStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalAssetReturnProposalStatus.Active:
                return next == RegionalAssetReturnProposalStatus.Closed;
            case RegionalAssetReturnProposalStatus.Suspended:
                switch (next)
                {
                    case RegionalAssetReturnProposalStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAssetReturnProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional asset return proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence * Ratio, 2);
    }

    public RegionalAssetReturnProposal Copy()
    {
        return new RegionalAssetReturnProposal(
            Code,
            Title,
            Skill,
            Threshold,
            Ratio,
            Tags,
            Sequence,
            WindowDays,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
