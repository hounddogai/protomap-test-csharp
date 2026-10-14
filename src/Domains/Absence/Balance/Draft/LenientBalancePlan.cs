namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient balance plan is in its life cycle.</summary>
public enum LenientBalancePlanStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient balance plan, with what the planners need to act on it.</summary>
public sealed class LenientBalancePlan(string code, string name, bool visible, int tenureMonths, string reviewer, int retries, string salaryBand, double maximum, double cost, int durationMinutes, LenientBalancePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public string SalaryBand { get; set; } = salaryBand;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LenientBalancePlanStatus Status { get; set; } = status;

    /// <summary>Starts a lenient balance plan in status planned.</summary>
    public static LenientBalancePlan NewLenientBalancePlan(string code, string name)
    {
        return new LenientBalancePlan(
            code,
            name,
            false,
            0,
            "",
            0,
            "",
            0.0,
            0.0,
            0,
            LenientBalancePlanStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBalancePlanStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBalancePlanStatus next)
    {
        switch (Status)
        {
            case LenientBalancePlanStatus.Planned:
                return next == LenientBalancePlanStatus.Scheduled || next == LenientBalancePlanStatus.InProgress;
            case LenientBalancePlanStatus.Scheduled:
                return next == LenientBalancePlanStatus.Cancelled;
            case LenientBalancePlanStatus.InProgress:
                return next == LenientBalancePlanStatus.Cancelled;
            case LenientBalancePlanStatus.Completed:
                return next == LenientBalancePlanStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientBalancePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient balance plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient balance plan"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes * (double)TenureMonths, 2);
    }

    public LenientBalancePlan Copy()
    {
        return new LenientBalancePlan(
            Code,
            Name,
            Visible,
            TenureMonths,
            Reviewer,
            Retries,
            SalaryBand,
            Maximum,
            Cost,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
