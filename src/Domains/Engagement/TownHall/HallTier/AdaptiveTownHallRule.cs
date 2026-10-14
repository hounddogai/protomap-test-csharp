namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive town hall rule is in its life cycle.</summary>
public enum AdaptiveTownHallRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive town hall rule, with what the planners need to act on it.</summary>
public sealed class AdaptiveTownHallRule(string code, string name, int revision, double ceiling, bool visible, string department, double floorValue, int createdAt, AdaptiveTownHallRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public AdaptiveTownHallRuleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive town hall rule in status open.</summary>
    public static AdaptiveTownHallRule NewAdaptiveTownHallRule(string code, string name)
    {
        return new AdaptiveTownHallRule(code, name, 0, 0.0, false, "", 0.0, 0, AdaptiveTownHallRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTownHallRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTownHallRuleStatus next)
    {
        switch (Status)
        {
            case AdaptiveTownHallRuleStatus.Open:
                return next == AdaptiveTownHallRuleStatus.InReview;
            case AdaptiveTownHallRuleStatus.InReview:
                switch (next)
                {
                    case AdaptiveTownHallRuleStatus.Resolved:
                        return true;
                }
                return false;
            case AdaptiveTownHallRuleStatus.Resolved:
                switch (next)
                {
                    case AdaptiveTownHallRuleStatus.Reopened:
                    case AdaptiveTownHallRuleStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveTownHallRuleStatus.Reopened:
                return next == AdaptiveTownHallRuleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveTownHallRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive town hall rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"adaptive town hall rule"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)CreatedAt - (double)Revision, 2);
    }

    public AdaptiveTownHallRule Copy()
    {
        return new AdaptiveTownHallRule(
            Code,
            Name,
            Revision,
            Ceiling,
            Visible,
            Department,
            FloorValue,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
