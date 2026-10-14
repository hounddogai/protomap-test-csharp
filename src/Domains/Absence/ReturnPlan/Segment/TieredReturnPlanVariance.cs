namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered return plan variance is in its life cycle.</summary>
public enum TieredReturnPlanVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered return plan variance as the return plan team keeps it.</summary>
public sealed class TieredReturnPlanVariance(string code, string name, string managerId, double fte, string channel, string region, double cost, int leaveDays, string summary, string category, TieredReturnPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public string Region { get; set; } = region;
    public double Cost { get; set; } = cost;
    public int LeaveDays { get; set; } = leaveDays;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public TieredReturnPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a tiered return plan variance in status planned.</summary>
    public static TieredReturnPlanVariance NewTieredReturnPlanVariance(string code, string name)
    {
        return new TieredReturnPlanVariance(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0.0,
            0,
            "",
            "",
            TieredReturnPlanVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredReturnPlanVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredReturnPlanVarianceStatus next)
    {
        switch (Status)
        {
            case TieredReturnPlanVarianceStatus.Planned:
                switch (next)
                {
                    case TieredReturnPlanVarianceStatus.Scheduled:
                    case TieredReturnPlanVarianceStatus.InProgress:
                        return true;
                }
                return false;
            case TieredReturnPlanVarianceStatus.Scheduled:
                switch (next)
                {
                    case TieredReturnPlanVarianceStatus.Completed:
                        return true;
                }
                return false;
            case TieredReturnPlanVarianceStatus.InProgress:
                switch (next)
                {
                    case TieredReturnPlanVarianceStatus.Completed:
                    case TieredReturnPlanVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredReturnPlanVarianceStatus.Completed:
                return next == TieredReturnPlanVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredReturnPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered return plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays + Cost, 2);
    }

    public TieredReturnPlanVariance Copy()
    {
        return new TieredReturnPlanVariance(
            Code,
            Name,
            ManagerId,
            Fte,
            Channel,
            Region,
            Cost,
            LeaveDays,
            Summary,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
