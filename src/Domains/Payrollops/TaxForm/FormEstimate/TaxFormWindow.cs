namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form window is in its life cycle.</summary>
public enum TaxFormWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tax form window. Its code is unique within a tenant.</summary>
public sealed class TaxFormWindow(string code, string name, string skill, int leaveDays, string employeeId, TaxFormWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public string EmployeeId { get; set; } = employeeId;
    public TaxFormWindowStatus Status { get; set; } = status;

    /// <summary>Starts a tax form window in status planned.</summary>
    public static TaxFormWindow NewTaxFormWindow(string code, string name)
    {
        return new TaxFormWindow(code, name, "", 0, "", TaxFormWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormWindowStatus next)
    {
        switch (Status)
        {
            case TaxFormWindowStatus.Planned:
                return next == TaxFormWindowStatus.InProgress || next == TaxFormWindowStatus.Cancelled;
            case TaxFormWindowStatus.Scheduled:
                return next == TaxFormWindowStatus.Completed;
            case TaxFormWindowStatus.InProgress:
                switch (next)
                {
                    case TaxFormWindowStatus.Completed:
                    case TaxFormWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case TaxFormWindowStatus.Completed:
                return next == TaxFormWindowStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TaxFormWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tax form window"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public TaxFormWindow Copy()
    {
        return new TaxFormWindow(Code, Name, Skill, LeaveDays, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
