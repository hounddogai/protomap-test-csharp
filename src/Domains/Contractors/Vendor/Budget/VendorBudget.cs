namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor budget is in its life cycle.</summary>
public enum VendorBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A vendor budget. Its code is unique within a tenant.</summary>
public sealed class VendorBudget(string code, string name, int windowDays, int graceDays, int limit, VendorBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public int Limit { get; set; } = limit;
    public VendorBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a vendor budget in status planned.</summary>
    public static VendorBudget NewVendorBudget(string code, string name)
    {
        return new VendorBudget(code, name, 0, 0, 0, VendorBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorBudgetStatus next)
    {
        switch (Status)
        {
            case VendorBudgetStatus.Planned:
                return next == VendorBudgetStatus.Completed;
            case VendorBudgetStatus.Scheduled:
                return next == VendorBudgetStatus.Cancelled;
            case VendorBudgetStatus.InProgress:
                switch (next)
                {
                    case VendorBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case VendorBudgetStatus.Completed:
                return next == VendorBudgetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(VendorBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"vendor budget"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)GraceDays + (double)WindowDays, 2);
    }

    public VendorBudget Copy()
    {
        return new VendorBudget(Code, Name, WindowDays, GraceDays, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
