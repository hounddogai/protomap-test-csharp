namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default payslip entry is in its life cycle.</summary>
public enum DefaultPayslipEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default payslip entry, with what the planners need to act on it.</summary>
public sealed class DefaultPayslipEntry(string code, string name, int windowDays, int attempts, int priority, string source, DefaultPayslipEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public string Source { get; set; } = source;
    public DefaultPayslipEntryStatus Status { get; set; } = status;

    /// <summary>Starts a default payslip entry in status draft.</summary>
    public static DefaultPayslipEntry NewDefaultPayslipEntry(string code, string name)
    {
        return new DefaultPayslipEntry(code, name, 0, 0, 0, "", DefaultPayslipEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayslipEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayslipEntryStatus next)
    {
        switch (Status)
        {
            case DefaultPayslipEntryStatus.Draft:
                switch (next)
                {
                    case DefaultPayslipEntryStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultPayslipEntryStatus.Pending:
                switch (next)
                {
                    case DefaultPayslipEntryStatus.Active:
                        return true;
                }
                return false;
            case DefaultPayslipEntryStatus.Active:
                return next == DefaultPayslipEntryStatus.Suspended || next == DefaultPayslipEntryStatus.Closed;
            case DefaultPayslipEntryStatus.Suspended:
                return next == DefaultPayslipEntryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultPayslipEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default payslip entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default payslip entry"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Attempts + (double)Priority, 2);
    }

    public DefaultPayslipEntry Copy()
    {
        return new DefaultPayslipEntry(Code, Name, WindowDays, Attempts, Priority, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
