namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual plan is in its life cycle.</summary>
public enum AccrualPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A accrual plan. Its code is unique within a tenant.</summary>
public sealed class AccrualPlan(string code, string title, double rating, string jobCode, int quantity, int priority, AccrualPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string JobCode { get; set; } = jobCode;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public AccrualPlanStatus Status { get; set; } = status;

    /// <summary>Starts a accrual plan in status created.</summary>
    public static AccrualPlan NewAccrualPlan(string code, string title)
    {
        return new AccrualPlan(code, title, 0.0, "", 0, 0, AccrualPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualPlanStatus next)
    {
        switch (Status)
        {
            case AccrualPlanStatus.Created:
                return next == AccrualPlanStatus.Published || next == AccrualPlanStatus.Retired;
            case AccrualPlanStatus.Validated:
                switch (next)
                {
                    case AccrualPlanStatus.Published:
                    case AccrualPlanStatus.Retired:
                        return true;
                }
                return false;
            case AccrualPlanStatus.Published:
                return next == AccrualPlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AccrualPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority - (double)Quantity, 2);
    }

    public AccrualPlan Copy()
    {
        return new AccrualPlan(Code, Title, Rating, JobCode, Quantity, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
