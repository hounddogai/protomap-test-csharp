namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered position proposal is in its life cycle.</summary>
public enum TieredPositionProposalStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered position proposal. Its code is unique within a tenant.</summary>
public sealed class TieredPositionProposal(string code, string label, int startsAt, double margin, int windowDays, string department, string skill, double hours, Dictionary<string, string> attributes, double cost, List<string> tags, TieredPositionProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Margin { get; set; } = margin;
    public int WindowDays { get; set; } = windowDays;
    public string Department { get; set; } = department;
    public string Skill { get; set; } = skill;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public TieredPositionProposalStatus Status { get; set; } = status;

    /// <summary>Starts a tiered position proposal in status created.</summary>
    public static TieredPositionProposal NewTieredPositionProposal(string code, string label)
    {
        return new TieredPositionProposal(
            code,
            label,
            0,
            0.0,
            0,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            0.0,
            new List<string>(),
            TieredPositionProposalStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPositionProposalStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPositionProposalStatus next)
    {
        switch (Status)
        {
            case TieredPositionProposalStatus.Created:
                switch (next)
                {
                    case TieredPositionProposalStatus.Validated:
                    case TieredPositionProposalStatus.Retired:
                        return true;
                }
                return false;
            case TieredPositionProposalStatus.Validated:
                switch (next)
                {
                    case TieredPositionProposalStatus.Published:
                    case TieredPositionProposalStatus.Retired:
                        return true;
                }
                return false;
            case TieredPositionProposalStatus.Published:
                switch (next)
                {
                    case TieredPositionProposalStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPositionProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered position proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"tiered position proposal"} {Code}";
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

    public double EffectiveVolume()
    {
        return Math.Round(Margin - (double)WindowDays, 2);
    }

    public TieredPositionProposal Copy()
    {
        return new TieredPositionProposal(
            Code,
            Label,
            StartsAt,
            Margin,
            WindowDays,
            Department,
            Skill,
            Hours,
            Attributes,
            Cost,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
