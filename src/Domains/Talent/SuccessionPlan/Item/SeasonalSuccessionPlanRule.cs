namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal succession plan rule is in its life cycle.</summary>
public enum SeasonalSuccessionPlanRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal succession plan rule as the succession plan team keeps it.</summary>
public sealed class SeasonalSuccessionPlanRule(string code, string title, int limit, int quantity, bool pinned, SeasonalSuccessionPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public SeasonalSuccessionPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal succession plan rule in status draft.</summary>
    public static SeasonalSuccessionPlanRule NewSeasonalSuccessionPlanRule(string code, string title)
    {
        return new SeasonalSuccessionPlanRule(code, title, 0, 0, false, SeasonalSuccessionPlanRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSuccessionPlanRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSuccessionPlanRuleStatus next)
    {
        switch (Status)
        {
            case SeasonalSuccessionPlanRuleStatus.Draft:
                return next == SeasonalSuccessionPlanRuleStatus.Active || next == SeasonalSuccessionPlanRuleStatus.Suspended;
            case SeasonalSuccessionPlanRuleStatus.Pending:
                switch (next)
                {
                    case SeasonalSuccessionPlanRuleStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalSuccessionPlanRuleStatus.Active:
                return next == SeasonalSuccessionPlanRuleStatus.Closed;
            case SeasonalSuccessionPlanRuleStatus.Suspended:
                return next == SeasonalSuccessionPlanRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalSuccessionPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal succession plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity * (double)Limit, 2);
    }

    public SeasonalSuccessionPlanRule Copy()
    {
        return new SeasonalSuccessionPlanRule(Code, Title, Limit, Quantity, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
