namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path plan is in its life cycle.</summary>
public enum CareerPathPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a career path plan and its current status.</summary>
public sealed class CareerPathPlan(string code, string label, string category, string costCenter, string locale, string title, int quantity, CareerPathPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string CostCenter { get; set; } = costCenter;
    public string Locale { get; set; } = locale;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public CareerPathPlanStatus Status { get; set; } = status;

    /// <summary>Starts a career path plan in status idle.</summary>
    public static CareerPathPlan NewCareerPathPlan(string code, string label)
    {
        return new CareerPathPlan(code, label, "", "", "", "", 0, CareerPathPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathPlanStatus next)
    {
        switch (Status)
        {
            case CareerPathPlanStatus.Idle:
                switch (next)
                {
                    case CareerPathPlanStatus.Draining:
                    case CareerPathPlanStatus.Stopped:
                        return true;
                }
                return false;
            case CareerPathPlanStatus.Warming:
                switch (next)
                {
                    case CareerPathPlanStatus.Ready:
                    case CareerPathPlanStatus.Stopped:
                        return true;
                }
                return false;
            case CareerPathPlanStatus.Ready:
                switch (next)
                {
                    case CareerPathPlanStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathPlanStatus.Draining:
                switch (next)
                {
                    case CareerPathPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"career path plan"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public CareerPathPlan Copy()
    {
        return new CareerPathPlan(Code, Label, Category, CostCenter, Locale, Title, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
