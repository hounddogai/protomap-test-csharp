namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pay run exception is in its life cycle.</summary>
public enum RollingPayRunExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a rolling pay run exception and its current status.</summary>
public sealed class RollingPayRunException(string code, string name, string department, double ceiling, string notes, string unit, bool approved, RollingPayRunExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public string Unit { get; set; } = unit;
    public bool Approved { get; set; } = approved;
    public RollingPayRunExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pay run exception in status draft.</summary>
    public static RollingPayRunException NewRollingPayRunException(string code, string name)
    {
        return new RollingPayRunException(code, name, "", 0.0, "", "", false, RollingPayRunExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayRunExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayRunExceptionStatus next)
    {
        switch (Status)
        {
            case RollingPayRunExceptionStatus.Draft:
                switch (next)
                {
                    case RollingPayRunExceptionStatus.Suspended:
                    case RollingPayRunExceptionStatus.Closed:
                        return true;
                }
                return false;
            case RollingPayRunExceptionStatus.Pending:
                return next == RollingPayRunExceptionStatus.Suspended;
            case RollingPayRunExceptionStatus.Active:
                return next == RollingPayRunExceptionStatus.Closed;
            case RollingPayRunExceptionStatus.Suspended:
                return next == RollingPayRunExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingPayRunExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pay run exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public RollingPayRunException Copy()
    {
        return new RollingPayRunException(Code, Name, Department, Ceiling, Notes, Unit, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
