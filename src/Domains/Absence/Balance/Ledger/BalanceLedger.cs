namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance ledger is in its life cycle.</summary>
public enum BalanceLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a balance ledger and its current status.</summary>
public sealed class BalanceLedger(string code, string label, int durationMinutes, string segment, int priority, BalanceLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public BalanceLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a balance ledger in status planned.</summary>
    public static BalanceLedger NewBalanceLedger(string code, string label)
    {
        return new BalanceLedger(code, label, 0, "", 0, BalanceLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceLedgerStatus next)
    {
        switch (Status)
        {
            case BalanceLedgerStatus.Planned:
                return next == BalanceLedgerStatus.Completed;
            case BalanceLedgerStatus.Scheduled:
                switch (next)
                {
                    case BalanceLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceLedgerStatus.InProgress:
                switch (next)
                {
                    case BalanceLedgerStatus.Completed:
                    case BalanceLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case BalanceLedgerStatus.Completed:
                switch (next)
                {
                    case BalanceLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"balance ledger"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)DurationMinutes - (double)Priority, 2);
    }

    public BalanceLedger Copy()
    {
        return new BalanceLedger(Code, Label, DurationMinutes, Segment, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
