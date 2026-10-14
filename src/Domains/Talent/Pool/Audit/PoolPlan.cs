namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool plan is in its life cycle.</summary>
public enum PoolPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pool plan and its current status.</summary>
public sealed class PoolPlan(string code, string label, string notes, int startsAt, double cost, int expiresAt, string badge, PoolPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Badge { get; set; } = badge;
    public PoolPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pool plan in status proposed.</summary>
    public static PoolPlan NewPoolPlan(string code, string label)
    {
        return new PoolPlan(code, label, "", 0, 0.0, 0, "", PoolPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolPlanStatus next)
    {
        switch (Status)
        {
            case PoolPlanStatus.Proposed:
                switch (next)
                {
                    case PoolPlanStatus.Approved:
                        return true;
                }
                return false;
            case PoolPlanStatus.Approved:
                switch (next)
                {
                    case PoolPlanStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolPlanStatus.Rejected:
                switch (next)
                {
                    case PoolPlanStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pool plan"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)ExpiresAt * Cost, 2);
    }

    public PoolPlan Copy()
    {
        return new PoolPlan(Code, Label, Notes, StartsAt, Cost, ExpiresAt, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
