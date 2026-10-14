namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered return plan rule is in its life cycle.</summary>
public enum TieredReturnPlanRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One tiered return plan rule, with what the planners need to act on it.</summary>
public sealed class TieredReturnPlanRule(string code, string name, double score, int expiresAt, string team, int updatedAt, double maximum, int leaveDays, double floorValue, TieredReturnPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Team { get; set; } = team;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public double FloorValue { get; set; } = floorValue;
    public TieredReturnPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered return plan rule in status idle.</summary>
    public static TieredReturnPlanRule NewTieredReturnPlanRule(string code, string name)
    {
        return new TieredReturnPlanRule(code, name, 0.0, 0, "", 0, 0.0, 0, 0.0, TieredReturnPlanRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredReturnPlanRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredReturnPlanRuleStatus next)
    {
        switch (Status)
        {
            case TieredReturnPlanRuleStatus.Idle:
                switch (next)
                {
                    case TieredReturnPlanRuleStatus.Ready:
                        return true;
                }
                return false;
            case TieredReturnPlanRuleStatus.Warming:
                switch (next)
                {
                    case TieredReturnPlanRuleStatus.Ready:
                    case TieredReturnPlanRuleStatus.Stopped:
                        return true;
                }
                return false;
            case TieredReturnPlanRuleStatus.Ready:
                switch (next)
                {
                    case TieredReturnPlanRuleStatus.Stopped:
                        return true;
                }
                return false;
            case TieredReturnPlanRuleStatus.Draining:
                switch (next)
                {
                    case TieredReturnPlanRuleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredReturnPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered return plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered return plan rule"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue * (double)LeaveDays, 2);
    }

    public TieredReturnPlanRule Copy()
    {
        return new TieredReturnPlanRule(
            Code,
            Name,
            Score,
            ExpiresAt,
            Team,
            UpdatedAt,
            Maximum,
            LeaveDays,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
