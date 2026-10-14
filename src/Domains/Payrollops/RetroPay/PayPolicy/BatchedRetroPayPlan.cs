namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched retro pay plan is in its life cycle.</summary>
public enum BatchedRetroPayPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched retro pay plan. Its code is unique within a tenant.</summary>
public sealed class BatchedRetroPayPlan(string code, string name, string managerId, bool active, int expiresAt, string costCenter, int attempts, double score, BatchedRetroPayPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public bool Active { get; set; } = active;
    public int ExpiresAt { get; set; } = expiresAt;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public double Score { get; set; } = score;
    public BatchedRetroPayPlanStatus Status { get; set; } = status;

    /// <summary>Starts a batched retro pay plan in status idle.</summary>
    public static BatchedRetroPayPlan NewBatchedRetroPayPlan(string code, string name)
    {
        return new BatchedRetroPayPlan(code, name, "", false, 0, "", 0, 0.0, BatchedRetroPayPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRetroPayPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRetroPayPlanStatus next)
    {
        switch (Status)
        {
            case BatchedRetroPayPlanStatus.Idle:
                switch (next)
                {
                    case BatchedRetroPayPlanStatus.Warming:
                    case BatchedRetroPayPlanStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedRetroPayPlanStatus.Warming:
                switch (next)
                {
                    case BatchedRetroPayPlanStatus.Ready:
                        return true;
                }
                return false;
            case BatchedRetroPayPlanStatus.Ready:
                switch (next)
                {
                    case BatchedRetroPayPlanStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedRetroPayPlanStatus.Draining:
                switch (next)
                {
                    case BatchedRetroPayPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRetroPayPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched retro pay plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Attempts + (double)ExpiresAt, 2);
    }

    public BatchedRetroPayPlan Copy()
    {
        return new BatchedRetroPayPlan(
            Code,
            Name,
            ManagerId,
            Active,
            ExpiresAt,
            CostCenter,
            Attempts,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
