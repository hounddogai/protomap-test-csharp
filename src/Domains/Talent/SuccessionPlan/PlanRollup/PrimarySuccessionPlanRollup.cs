namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary succession plan rollup is in its life cycle.</summary>
public enum PrimarySuccessionPlanRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary succession plan rollup. Its code is unique within a tenant.</summary>
public sealed class PrimarySuccessionPlanRollup(string code, string label, string channel, bool active, int limit, double minimum, int quantity, int version, PrimarySuccessionPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public bool Active { get; set; } = active;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public PrimarySuccessionPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a primary succession plan rollup in status open.</summary>
    public static PrimarySuccessionPlanRollup NewPrimarySuccessionPlanRollup(string code, string label)
    {
        return new PrimarySuccessionPlanRollup(
            code,
            label,
            "",
            false,
            0,
            0.0,
            0,
            0,
            PrimarySuccessionPlanRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySuccessionPlanRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySuccessionPlanRollupStatus next)
    {
        switch (Status)
        {
            case PrimarySuccessionPlanRollupStatus.Open:
                switch (next)
                {
                    case PrimarySuccessionPlanRollupStatus.Archived:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanRollupStatus.InReview:
                return next == PrimarySuccessionPlanRollupStatus.Resolved || next == PrimarySuccessionPlanRollupStatus.Archived;
            case PrimarySuccessionPlanRollupStatus.Resolved:
                return next == PrimarySuccessionPlanRollupStatus.Reopened || next == PrimarySuccessionPlanRollupStatus.Archived;
            case PrimarySuccessionPlanRollupStatus.Reopened:
                return next == PrimarySuccessionPlanRollupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimarySuccessionPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary succession plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"primary succession plan rollup"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Quantity - (double)Version, 2);
    }

    public PrimarySuccessionPlanRollup Copy()
    {
        return new PrimarySuccessionPlanRollup(
            Code,
            Label,
            Channel,
            Active,
            Limit,
            Minimum,
            Quantity,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
