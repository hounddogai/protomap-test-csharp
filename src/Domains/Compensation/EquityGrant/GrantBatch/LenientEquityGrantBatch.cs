namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient equity grant batch is in its life cycle.</summary>
public enum LenientEquityGrantBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient equity grant batch as the equity grant team keeps it.</summary>
public sealed class LenientEquityGrantBatch(string code, string name, string jobCode, double ceiling, string unit, LenientEquityGrantBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public double Ceiling { get; set; } = ceiling;
    public string Unit { get; set; } = unit;
    public LenientEquityGrantBatchStatus Status { get; set; } = status;

    /// <summary>Starts a lenient equity grant batch in status proposed.</summary>
    public static LenientEquityGrantBatch NewLenientEquityGrantBatch(string code, string name)
    {
        return new LenientEquityGrantBatch(code, name, "", 0.0, "", LenientEquityGrantBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientEquityGrantBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientEquityGrantBatchStatus next)
    {
        switch (Status)
        {
            case LenientEquityGrantBatchStatus.Proposed:
                return next == LenientEquityGrantBatchStatus.Approved || next == LenientEquityGrantBatchStatus.Rejected;
            case LenientEquityGrantBatchStatus.Approved:
                switch (next)
                {
                    case LenientEquityGrantBatchStatus.Rejected:
                    case LenientEquityGrantBatchStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientEquityGrantBatchStatus.Rejected:
                return next == LenientEquityGrantBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientEquityGrantBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient equity grant batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public LenientEquityGrantBatch Copy()
    {
        return new LenientEquityGrantBatch(Code, Name, JobCode, Ceiling, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
