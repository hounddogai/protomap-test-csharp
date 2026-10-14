namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay run ledger is in its life cycle.</summary>
public enum TieredPayRunLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered pay run ledger as the pay run team keeps it.</summary>
public sealed class TieredPayRunLedger(string code, string name, int windowDays, int startsAt, string summary, string label, int capacity, int leaveDays, int durationMinutes, double amount, double weight, TieredPayRunLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Summary { get; set; } = summary;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public int LeaveDays { get; set; } = leaveDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Amount { get; set; } = amount;
    public double Weight { get; set; } = weight;
    public TieredPayRunLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay run ledger in status open.</summary>
    public static TieredPayRunLedger NewTieredPayRunLedger(string code, string name)
    {
        return new TieredPayRunLedger(code, name, 0, 0, "", "", 0, 0, 0, 0.0, 0.0, TieredPayRunLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayRunLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayRunLedgerStatus next)
    {
        switch (Status)
        {
            case TieredPayRunLedgerStatus.Open:
                return next == TieredPayRunLedgerStatus.Archived;
            case TieredPayRunLedgerStatus.InReview:
                return next == TieredPayRunLedgerStatus.Reopened;
            case TieredPayRunLedgerStatus.Resolved:
                return next == TieredPayRunLedgerStatus.Reopened || next == TieredPayRunLedgerStatus.Archived;
            case TieredPayRunLedgerStatus.Reopened:
                return next == TieredPayRunLedgerStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredPayRunLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay run ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes + (double)LeaveDays, 2);
    }

    public TieredPayRunLedger Copy()
    {
        return new TieredPayRunLedger(
            Code,
            Name,
            WindowDays,
            StartsAt,
            Summary,
            Label,
            Capacity,
            LeaveDays,
            DurationMinutes,
            Amount,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
