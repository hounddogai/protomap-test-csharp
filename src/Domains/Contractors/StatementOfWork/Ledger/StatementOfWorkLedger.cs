namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work ledger is in its life cycle.</summary>
public enum StatementOfWorkLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A statement of work ledger as the statement of work team keeps it.</summary>
public sealed class StatementOfWorkLedger(string code, string label, string name, string summary, string managerId, double rate, StatementOfWorkLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public StatementOfWorkLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work ledger in status planned.</summary>
    public static StatementOfWorkLedger NewStatementOfWorkLedger(string code, string label)
    {
        return new StatementOfWorkLedger(code, label, "", "", "", 0.0, StatementOfWorkLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkLedgerStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkLedgerStatus.Planned:
                switch (next)
                {
                    case StatementOfWorkLedgerStatus.Scheduled:
                        return true;
                }
                return false;
            case StatementOfWorkLedgerStatus.Scheduled:
                return next == StatementOfWorkLedgerStatus.Completed;
            case StatementOfWorkLedgerStatus.InProgress:
                return next == StatementOfWorkLedgerStatus.Completed || next == StatementOfWorkLedgerStatus.Cancelled;
            case StatementOfWorkLedgerStatus.Completed:
                return next == StatementOfWorkLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public StatementOfWorkLedger Copy()
    {
        return new StatementOfWorkLedger(Code, Label, Name, Summary, ManagerId, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
