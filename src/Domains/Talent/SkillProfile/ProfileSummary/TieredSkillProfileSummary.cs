namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered skill profile summary is in its life cycle.</summary>
public enum TieredSkillProfileSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered skill profile summary, with what the planners need to act on it.</summary>
public sealed class TieredSkillProfileSummary(string code, string title, string badge, Dictionary<string, string> attributes, string name, int graceDays, string team, double weight, string segment, string description, TieredSkillProfileSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Team { get; set; } = team;
    public double Weight { get; set; } = weight;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public TieredSkillProfileSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered skill profile summary in status draft.</summary>
    public static TieredSkillProfileSummary NewTieredSkillProfileSummary(string code, string title)
    {
        return new TieredSkillProfileSummary(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            "",
            0.0,
            "",
            "",
            TieredSkillProfileSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSkillProfileSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSkillProfileSummaryStatus next)
    {
        switch (Status)
        {
            case TieredSkillProfileSummaryStatus.Draft:
                return next == TieredSkillProfileSummaryStatus.Closed;
            case TieredSkillProfileSummaryStatus.Pending:
                return next == TieredSkillProfileSummaryStatus.Suspended || next == TieredSkillProfileSummaryStatus.Closed;
            case TieredSkillProfileSummaryStatus.Active:
                switch (next)
                {
                    case TieredSkillProfileSummaryStatus.Suspended:
                    case TieredSkillProfileSummaryStatus.Closed:
                        return true;
                }
                return false;
            case TieredSkillProfileSummaryStatus.Suspended:
                switch (next)
                {
                    case TieredSkillProfileSummaryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSkillProfileSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered skill profile summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double WeightedTotal()
    {
        return Math.Round(Weight * (double)GraceDays, 2);
    }

    public TieredSkillProfileSummary Copy()
    {
        return new TieredSkillProfileSummary(
            Code,
            Title,
            Badge,
            Attributes,
            Name,
            GraceDays,
            Team,
            Weight,
            Segment,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
