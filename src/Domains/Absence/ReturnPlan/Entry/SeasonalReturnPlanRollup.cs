namespace Acme.Workforce.Absence.ReturnPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal return plan rollup is in its life cycle.</summary>
public enum SeasonalReturnPlanRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One seasonal return plan rollup, with what the planners need to act on it.</summary>
public sealed class SeasonalReturnPlanRollup(string code, string name, bool visible, int revision, double amount, bool archived, string channel, string segment, string location, string timezone, double weight, SeasonalReturnPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int Revision { get; set; } = revision;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public string Timezone { get; set; } = timezone;
    public double Weight { get; set; } = weight;
    public SeasonalReturnPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal return plan rollup in status proposed.</summary>
    public static SeasonalReturnPlanRollup NewSeasonalReturnPlanRollup(string code, string name)
    {
        return new SeasonalReturnPlanRollup(
            code,
            name,
            false,
            0,
            0.0,
            false,
            "",
            "",
            "",
            "",
            0.0,
            SeasonalReturnPlanRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalReturnPlanRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalReturnPlanRollupStatus next)
    {
        switch (Status)
        {
            case SeasonalReturnPlanRollupStatus.Proposed:
                switch (next)
                {
                    case SeasonalReturnPlanRollupStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalReturnPlanRollupStatus.Approved:
                return next == SeasonalReturnPlanRollupStatus.Rejected || next == SeasonalReturnPlanRollupStatus.Withdrawn;
            case SeasonalReturnPlanRollupStatus.Rejected:
                switch (next)
                {
                    case SeasonalReturnPlanRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalReturnPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal return plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Amount * Weight, 2);
    }

    public SeasonalReturnPlanRollup Copy()
    {
        return new SeasonalReturnPlanRollup(
            Code,
            Name,
            Visible,
            Revision,
            Amount,
            Archived,
            Channel,
            Segment,
            Location,
            Timezone,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
