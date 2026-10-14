namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered timesheet budget is in its life cycle.</summary>
public enum TieredTimesheetBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered timesheet budget. Its code is unique within a tenant.</summary>
public sealed class TieredTimesheetBudget(string code, string name, string department, double ceiling, double fte, double minimum, TieredTimesheetBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public double Fte { get; set; } = fte;
    public double Minimum { get; set; } = minimum;
    public TieredTimesheetBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a tiered timesheet budget in status idle.</summary>
    public static TieredTimesheetBudget NewTieredTimesheetBudget(string code, string name)
    {
        return new TieredTimesheetBudget(code, name, "", 0.0, 0.0, 0.0, TieredTimesheetBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredTimesheetBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredTimesheetBudgetStatus next)
    {
        switch (Status)
        {
            case TieredTimesheetBudgetStatus.Idle:
                return next == TieredTimesheetBudgetStatus.Draining;
            case TieredTimesheetBudgetStatus.Warming:
                switch (next)
                {
                    case TieredTimesheetBudgetStatus.Draining:
                    case TieredTimesheetBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case TieredTimesheetBudgetStatus.Ready:
                return next == TieredTimesheetBudgetStatus.Stopped;
            case TieredTimesheetBudgetStatus.Draining:
                return next == TieredTimesheetBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredTimesheetBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered timesheet budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered timesheet budget"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling + Fte, 2);
    }

    public TieredTimesheetBudget Copy()
    {
        return new TieredTimesheetBudget(Code, Name, Department, Ceiling, Fte, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
