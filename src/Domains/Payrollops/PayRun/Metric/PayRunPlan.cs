namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run plan is in its life cycle.</summary>
public enum PayRunPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pay run plan and its current status.</summary>
public sealed class PayRunPlan(string code, string name, double rate, double maximum, string badge, string unit, int limit, double weight, string externalRef, PayRunPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Maximum { get; set; } = maximum;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public int Limit { get; set; } = limit;
    public double Weight { get; set; } = weight;
    public string ExternalRef { get; set; } = externalRef;
    public PayRunPlanStatus Status { get; set; } = status;

    /// <summary>Starts a pay run plan in status draft.</summary>
    public static PayRunPlan NewPayRunPlan(string code, string name)
    {
        return new PayRunPlan(code, name, 0.0, 0.0, "", "", 0, 0.0, "", PayRunPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunPlanStatus next)
    {
        switch (Status)
        {
            case PayRunPlanStatus.Draft:
                switch (next)
                {
                    case PayRunPlanStatus.Suspended:
                    case PayRunPlanStatus.Closed:
                        return true;
                }
                return false;
            case PayRunPlanStatus.Pending:
                switch (next)
                {
                    case PayRunPlanStatus.Closed:
                        return true;
                }
                return false;
            case PayRunPlanStatus.Active:
                return next == PayRunPlanStatus.Closed;
            case PayRunPlanStatus.Suspended:
                return next == PayRunPlanStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayRunPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay run plan"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Limit - Rate, 2);
    }

    public PayRunPlan Copy()
    {
        return new PayRunPlan(Code, Name, Rate, Maximum, Badge, Unit, Limit, Weight, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
