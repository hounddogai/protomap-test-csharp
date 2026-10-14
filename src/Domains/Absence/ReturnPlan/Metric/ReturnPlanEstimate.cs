namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan estimate is in its life cycle.</summary>
public enum ReturnPlanEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A return plan estimate. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanEstimate(string code, string name, bool archived, double cost, double amount, double score, int tenureMonths, string timezone, string externalRef, string unit, int limit, ReturnPlanEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Cost { get; set; } = cost;
    public double Amount { get; set; } = amount;
    public double Score { get; set; } = score;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public string Unit { get; set; } = unit;
    public int Limit { get; set; } = limit;
    public ReturnPlanEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a return plan estimate in status planned.</summary>
    public static ReturnPlanEstimate NewReturnPlanEstimate(string code, string name)
    {
        return new ReturnPlanEstimate(
            code,
            name,
            false,
            0.0,
            0.0,
            0.0,
            0,
            "",
            "",
            "",
            0,
            ReturnPlanEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanEstimateStatus next)
    {
        switch (Status)
        {
            case ReturnPlanEstimateStatus.Planned:
                switch (next)
                {
                    case ReturnPlanEstimateStatus.Scheduled:
                        return true;
                }
                return false;
            case ReturnPlanEstimateStatus.Scheduled:
                return next == ReturnPlanEstimateStatus.Completed;
            case ReturnPlanEstimateStatus.InProgress:
                return next == ReturnPlanEstimateStatus.Completed;
            case ReturnPlanEstimateStatus.Completed:
                return next == ReturnPlanEstimateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ReturnPlanEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"return plan estimate"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Score - Amount, 2);
    }

    public ReturnPlanEstimate Copy()
    {
        return new ReturnPlanEstimate(
            Code,
            Name,
            Archived,
            Cost,
            Amount,
            Score,
            TenureMonths,
            Timezone,
            ExternalRef,
            Unit,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
