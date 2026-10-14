namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled equity grant ledger is in its life cycle.</summary>
public enum ScheduledEquityGrantLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled equity grant ledger, with what the planners need to act on it.</summary>
public sealed class ScheduledEquityGrantLedger(string code, string label, int graceDays, int expiresAt, string locale, ScheduledEquityGrantLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Locale { get; set; } = locale;
    public ScheduledEquityGrantLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled equity grant ledger in status proposed.</summary>
    public static ScheduledEquityGrantLedger NewScheduledEquityGrantLedger(string code, string label)
    {
        return new ScheduledEquityGrantLedger(code, label, 0, 0, "", ScheduledEquityGrantLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEquityGrantLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEquityGrantLedgerStatus next)
    {
        switch (Status)
        {
            case ScheduledEquityGrantLedgerStatus.Proposed:
                return next == ScheduledEquityGrantLedgerStatus.Withdrawn;
            case ScheduledEquityGrantLedgerStatus.Approved:
                switch (next)
                {
                    case ScheduledEquityGrantLedgerStatus.Rejected:
                    case ScheduledEquityGrantLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledEquityGrantLedgerStatus.Rejected:
                return next == ScheduledEquityGrantLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledEquityGrantLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled equity grant ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"scheduled equity grant ledger"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)GraceDays * (double)ExpiresAt, 2);
    }

    public ScheduledEquityGrantLedger Copy()
    {
        return new ScheduledEquityGrantLedger(Code, Label, GraceDays, ExpiresAt, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
