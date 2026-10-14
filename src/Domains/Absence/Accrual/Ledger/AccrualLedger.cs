namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual ledger is in its life cycle.</summary>
public enum AccrualLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One accrual ledger, with what the planners need to act on it.</summary>
public sealed class AccrualLedger(string code, string label, int batchSize, int windowDays, string segment, string managerId, double cost, double minimum, int retries, AccrualLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public string ManagerId { get; set; } = managerId;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public AccrualLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a accrual ledger in status planned.</summary>
    public static AccrualLedger NewAccrualLedger(string code, string label)
    {
        return new AccrualLedger(code, label, 0, 0, "", "", 0.0, 0.0, 0, AccrualLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualLedgerStatus next)
    {
        switch (Status)
        {
            case AccrualLedgerStatus.Planned:
                switch (next)
                {
                    case AccrualLedgerStatus.InProgress:
                    case AccrualLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualLedgerStatus.Scheduled:
                return next == AccrualLedgerStatus.Cancelled;
            case AccrualLedgerStatus.InProgress:
                switch (next)
                {
                    case AccrualLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualLedgerStatus.Completed:
                return next == AccrualLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AccrualLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Minimum * (double)BatchSize, 2);
    }

    public AccrualLedger Copy()
    {
        return new AccrualLedger(
            Code,
            Label,
            BatchSize,
            WindowDays,
            Segment,
            ManagerId,
            Cost,
            Minimum,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
