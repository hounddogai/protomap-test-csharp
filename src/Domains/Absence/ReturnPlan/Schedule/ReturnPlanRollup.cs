namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan rollup is in its life cycle.</summary>
public enum ReturnPlanRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A return plan rollup as the return plan team keeps it.</summary>
public sealed class ReturnPlanRollup(string code, string title, double cost, string team, string unit, int updatedAt, string currency, string category, int priority, string channel, ReturnPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string Team { get; set; } = team;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public string Category { get; set; } = category;
    public int Priority { get; set; } = priority;
    public string Channel { get; set; } = channel;
    public ReturnPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a return plan rollup in status proposed.</summary>
    public static ReturnPlanRollup NewReturnPlanRollup(string code, string title)
    {
        return new ReturnPlanRollup(code, title, 0.0, "", "", 0, "", "", 0, "", ReturnPlanRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanRollupStatus next)
    {
        switch (Status)
        {
            case ReturnPlanRollupStatus.Proposed:
                return next == ReturnPlanRollupStatus.Approved || next == ReturnPlanRollupStatus.Rejected;
            case ReturnPlanRollupStatus.Approved:
                switch (next)
                {
                    case ReturnPlanRollupStatus.Rejected:
                    case ReturnPlanRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case ReturnPlanRollupStatus.Rejected:
                switch (next)
                {
                    case ReturnPlanRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"return plan rollup"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)UpdatedAt - Cost, 2);
    }

    public ReturnPlanRollup Copy()
    {
        return new ReturnPlanRollup(
            Code,
            Title,
            Cost,
            Team,
            Unit,
            UpdatedAt,
            Currency,
            Category,
            Priority,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
