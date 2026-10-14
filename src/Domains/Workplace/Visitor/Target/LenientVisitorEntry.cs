namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient visitor entry is in its life cycle.</summary>
public enum LenientVisitorEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient visitor entry and its current status.</summary>
public sealed class LenientVisitorEntry(string code, string name, string jobCode, double threshold, string salaryBand, string unit, int graceDays, string managerId, string externalRef, double ceiling, LenientVisitorEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public double Ceiling { get; set; } = ceiling;
    public LenientVisitorEntryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient visitor entry in status planned.</summary>
    public static LenientVisitorEntry NewLenientVisitorEntry(string code, string name)
    {
        return new LenientVisitorEntry(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0,
            "",
            "",
            0.0,
            LenientVisitorEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVisitorEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVisitorEntryStatus next)
    {
        switch (Status)
        {
            case LenientVisitorEntryStatus.Planned:
                return next == LenientVisitorEntryStatus.InProgress || next == LenientVisitorEntryStatus.Cancelled;
            case LenientVisitorEntryStatus.Scheduled:
                switch (next)
                {
                    case LenientVisitorEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientVisitorEntryStatus.InProgress:
                return next == LenientVisitorEntryStatus.Completed || next == LenientVisitorEntryStatus.Cancelled;
            case LenientVisitorEntryStatus.Completed:
                return next == LenientVisitorEntryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientVisitorEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient visitor entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round((double)GraceDays - Ceiling, 2);
    }

    public LenientVisitorEntry Copy()
    {
        return new LenientVisitorEntry(
            Code,
            Name,
            JobCode,
            Threshold,
            SalaryBand,
            Unit,
            GraceDays,
            ManagerId,
            ExternalRef,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
