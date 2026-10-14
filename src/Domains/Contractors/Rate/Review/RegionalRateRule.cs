namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional rate rule is in its life cycle.</summary>
public enum RegionalRateRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional rate rule. Its code is unique within a tenant.</summary>
public sealed class RegionalRateRule(string code, string title, double weight, int limit, int priority, double fte, int sequence, RegionalRateRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int Limit { get; set; } = limit;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public int Sequence { get; set; } = sequence;
    public RegionalRateRuleStatus Status { get; set; } = status;

    /// <summary>Starts a regional rate rule in status planned.</summary>
    public static RegionalRateRule NewRegionalRateRule(string code, string title)
    {
        return new RegionalRateRule(code, title, 0.0, 0, 0, 0.0, 0, RegionalRateRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalRateRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalRateRuleStatus next)
    {
        switch (Status)
        {
            case RegionalRateRuleStatus.Planned:
                switch (next)
                {
                    case RegionalRateRuleStatus.Scheduled:
                    case RegionalRateRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalRateRuleStatus.Scheduled:
                switch (next)
                {
                    case RegionalRateRuleStatus.Completed:
                        return true;
                }
                return false;
            case RegionalRateRuleStatus.InProgress:
                return next == RegionalRateRuleStatus.Completed || next == RegionalRateRuleStatus.Cancelled;
            case RegionalRateRuleStatus.Completed:
                switch (next)
                {
                    case RegionalRateRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalRateRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional rate rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte - (double)Limit, 2);
    }

    public RegionalRateRule Copy()
    {
        return new RegionalRateRule(Code, Title, Weight, Limit, Priority, Fte, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
