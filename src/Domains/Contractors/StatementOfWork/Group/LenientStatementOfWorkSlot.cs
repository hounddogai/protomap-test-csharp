namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient statement of work slot is in its life cycle.</summary>
public enum LenientStatementOfWorkSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient statement of work slot as the statement of work team keeps it.</summary>
public sealed class LenientStatementOfWorkSlot(string code, string name, int tenureMonths, string source, string employeeId, int graceDays, double cost, string currency, double margin, LenientStatementOfWorkSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Source { get; set; } = source;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public double Margin { get; set; } = margin;
    public LenientStatementOfWorkSlotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient statement of work slot in status planned.</summary>
    public static LenientStatementOfWorkSlot NewLenientStatementOfWorkSlot(string code, string name)
    {
        return new LenientStatementOfWorkSlot(
            code,
            name,
            0,
            "",
            "",
            0,
            0.0,
            "",
            0.0,
            LenientStatementOfWorkSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientStatementOfWorkSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientStatementOfWorkSlotStatus next)
    {
        switch (Status)
        {
            case LenientStatementOfWorkSlotStatus.Planned:
                switch (next)
                {
                    case LenientStatementOfWorkSlotStatus.Scheduled:
                    case LenientStatementOfWorkSlotStatus.InProgress:
                        return true;
                }
                return false;
            case LenientStatementOfWorkSlotStatus.Scheduled:
                return next == LenientStatementOfWorkSlotStatus.Completed;
            case LenientStatementOfWorkSlotStatus.InProgress:
                switch (next)
                {
                    case LenientStatementOfWorkSlotStatus.Completed:
                        return true;
                }
                return false;
            case LenientStatementOfWorkSlotStatus.Completed:
                return next == LenientStatementOfWorkSlotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientStatementOfWorkSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient statement of work slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Margin * (double)GraceDays, 2);
    }

    public LenientStatementOfWorkSlot Copy()
    {
        return new LenientStatementOfWorkSlot(
            Code,
            Name,
            TenureMonths,
            Source,
            EmployeeId,
            GraceDays,
            Cost,
            Currency,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
