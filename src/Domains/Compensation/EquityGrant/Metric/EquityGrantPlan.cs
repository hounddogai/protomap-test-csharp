namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant plan is in its life cycle.</summary>
public enum EquityGrantPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A equity grant plan as the equity grant team keeps it.</summary>
public sealed class EquityGrantPlan(string code, string title, string reviewer, double fte, string channel, string employeeId, int retries, string salaryBand, string unit, string category, EquityGrantPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public string Category { get; set; } = category;
    public EquityGrantPlanStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant plan in status idle.</summary>
    public static EquityGrantPlan NewEquityGrantPlan(string code, string title)
    {
        return new EquityGrantPlan(code, title, "", 0.0, "", "", 0, "", "", "", EquityGrantPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantPlanStatus next)
    {
        switch (Status)
        {
            case EquityGrantPlanStatus.Idle:
                switch (next)
                {
                    case EquityGrantPlanStatus.Stopped:
                        return true;
                }
                return false;
            case EquityGrantPlanStatus.Warming:
                return next == EquityGrantPlanStatus.Ready;
            case EquityGrantPlanStatus.Ready:
                return next == EquityGrantPlanStatus.Draining || next == EquityGrantPlanStatus.Stopped;
            case EquityGrantPlanStatus.Draining:
                return next == EquityGrantPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(EquityGrantPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Fte + (double)Retries, 2);
    }

    public EquityGrantPlan Copy()
    {
        return new EquityGrantPlan(
            Code,
            Title,
            Reviewer,
            Fte,
            Channel,
            EmployeeId,
            Retries,
            SalaryBand,
            Unit,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
