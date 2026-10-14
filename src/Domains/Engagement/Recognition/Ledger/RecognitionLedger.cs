namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition ledger is in its life cycle.</summary>
public enum RecognitionLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A recognition ledger as the recognition team keeps it.</summary>
public sealed class RecognitionLedger(string code, string label, string unit, int priority, string salaryBand, string summary, int tenureMonths, string channel, int leaveDays, int limit, RecognitionLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public string SalaryBand { get; set; } = salaryBand;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public RecognitionLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a recognition ledger in status open.</summary>
    public static RecognitionLedger NewRecognitionLedger(string code, string label)
    {
        return new RecognitionLedger(code, label, "", 0, "", "", 0, "", 0, 0, RecognitionLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionLedgerStatus next)
    {
        switch (Status)
        {
            case RecognitionLedgerStatus.Open:
                switch (next)
                {
                    case RecognitionLedgerStatus.Resolved:
                    case RecognitionLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case RecognitionLedgerStatus.InReview:
                return next == RecognitionLedgerStatus.Resolved;
            case RecognitionLedgerStatus.Resolved:
                switch (next)
                {
                    case RecognitionLedgerStatus.Archived:
                        return true;
                }
                return false;
            case RecognitionLedgerStatus.Reopened:
                return next == RecognitionLedgerStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RecognitionLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"recognition ledger"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)TenureMonths - (double)LeaveDays, 2);
    }

    public RecognitionLedger Copy()
    {
        return new RecognitionLedger(
            Code,
            Label,
            Unit,
            Priority,
            SalaryBand,
            Summary,
            TenureMonths,
            Channel,
            LeaveDays,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
