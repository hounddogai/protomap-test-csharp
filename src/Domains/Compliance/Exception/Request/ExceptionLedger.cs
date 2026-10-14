namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception ledger is in its life cycle.</summary>
public enum ExceptionLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a exception ledger and its current status.</summary>
public sealed class ExceptionLedger(string code, string label, int limit, double minimum, double margin, string badge, ExceptionLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public ExceptionLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a exception ledger in status planned.</summary>
    public static ExceptionLedger NewExceptionLedger(string code, string label)
    {
        return new ExceptionLedger(code, label, 0, 0.0, 0.0, "", ExceptionLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionLedgerStatus next)
    {
        switch (Status)
        {
            case ExceptionLedgerStatus.Planned:
                switch (next)
                {
                    case ExceptionLedgerStatus.Completed:
                        return true;
                }
                return false;
            case ExceptionLedgerStatus.Scheduled:
                return next == ExceptionLedgerStatus.InProgress || next == ExceptionLedgerStatus.Cancelled;
            case ExceptionLedgerStatus.InProgress:
                switch (next)
                {
                    case ExceptionLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case ExceptionLedgerStatus.Completed:
                return next == ExceptionLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ExceptionLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Minimum + Margin, 2);
    }

    public ExceptionLedger Copy()
    {
        return new ExceptionLedger(Code, Label, Limit, Minimum, Margin, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
