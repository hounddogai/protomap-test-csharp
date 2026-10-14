namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global mobility rule is in its life cycle.</summary>
public enum GlobalMobilityRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a global mobility rule and its current status.</summary>
public sealed class GlobalMobilityRule(string code, string title, string location, double ratio, int priority, int capacity, string costCenter, int windowDays, string channel, string contactEmail, GlobalMobilityRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public double Ratio { get; set; } = ratio;
    public int Priority { get; set; } = priority;
    public int Capacity { get; set; } = capacity;
    public string CostCenter { get; set; } = costCenter;
    public int WindowDays { get; set; } = windowDays;
    public string Channel { get; set; } = channel;
    public string ContactEmail { get; set; } = contactEmail;
    public GlobalMobilityRuleStatus Status { get; set; } = status;

    /// <summary>Starts a global mobility rule in status draft.</summary>
    public static GlobalMobilityRule NewGlobalMobilityRule(string code, string title)
    {
        return new GlobalMobilityRule(code, title, "", 0.0, 0, 0, "", 0, "", "", GlobalMobilityRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalMobilityRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalMobilityRuleStatus next)
    {
        switch (Status)
        {
            case GlobalMobilityRuleStatus.Draft:
                return next == GlobalMobilityRuleStatus.Pending || next == GlobalMobilityRuleStatus.Active;
            case GlobalMobilityRuleStatus.Pending:
                switch (next)
                {
                    case GlobalMobilityRuleStatus.Active:
                    case GlobalMobilityRuleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalMobilityRuleStatus.Active:
                switch (next)
                {
                    case GlobalMobilityRuleStatus.Suspended:
                    case GlobalMobilityRuleStatus.Closed:
                        return true;
                }
                return false;
            case GlobalMobilityRuleStatus.Suspended:
                return next == GlobalMobilityRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalMobilityRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global mobility rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Priority + Ratio, 2);
    }

    public GlobalMobilityRule Copy()
    {
        return new GlobalMobilityRule(
            Code,
            Title,
            Location,
            Ratio,
            Priority,
            Capacity,
            CostCenter,
            WindowDays,
            Channel,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
