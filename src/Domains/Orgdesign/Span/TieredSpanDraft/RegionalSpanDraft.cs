namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional span draft is in its life cycle.</summary>
public enum RegionalSpanDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional span draft. Its code is unique within a tenant.</summary>
public sealed class RegionalSpanDraft(string code, string name, List<string> tags, string currency, double cost, string badge, double maximum, string skill, double weight, RegionalSpanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Currency { get; set; } = currency;
    public double Cost { get; set; } = cost;
    public string Badge { get; set; } = badge;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public double Weight { get; set; } = weight;
    public RegionalSpanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a regional span draft in status idle.</summary>
    public static RegionalSpanDraft NewRegionalSpanDraft(string code, string name)
    {
        return new RegionalSpanDraft(
            code,
            name,
            new List<string>(),
            "",
            0.0,
            "",
            0.0,
            "",
            0.0,
            RegionalSpanDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSpanDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSpanDraftStatus next)
    {
        switch (Status)
        {
            case RegionalSpanDraftStatus.Idle:
                switch (next)
                {
                    case RegionalSpanDraftStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalSpanDraftStatus.Warming:
                return next == RegionalSpanDraftStatus.Stopped;
            case RegionalSpanDraftStatus.Ready:
                return next == RegionalSpanDraftStatus.Stopped;
            case RegionalSpanDraftStatus.Draining:
                return next == RegionalSpanDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalSpanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional span draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Maximum * Cost, 2);
    }

    public RegionalSpanDraft Copy()
    {
        return new RegionalSpanDraft(Code, Name, Tags, Currency, Cost, Badge, Maximum, Skill, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
