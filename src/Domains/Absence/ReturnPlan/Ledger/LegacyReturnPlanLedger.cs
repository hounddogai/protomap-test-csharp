namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy return plan ledger is in its life cycle.</summary>
public enum LegacyReturnPlanLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy return plan ledger. Its code is unique within a tenant.</summary>
public sealed class LegacyReturnPlanLedger(string code, string label, int tenureMonths, string unit, string grade, string timezone, string currency, LegacyReturnPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Unit { get; set; } = unit;
    public string Grade { get; set; } = grade;
    public string Timezone { get; set; } = timezone;
    public string Currency { get; set; } = currency;
    public LegacyReturnPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a legacy return plan ledger in status new.</summary>
    public static LegacyReturnPlanLedger NewLegacyReturnPlanLedger(string code, string label)
    {
        return new LegacyReturnPlanLedger(code, label, 0, "", "", "", "", LegacyReturnPlanLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyReturnPlanLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyReturnPlanLedgerStatus next)
    {
        switch (Status)
        {
            case LegacyReturnPlanLedgerStatus.New:
                return next == LegacyReturnPlanLedgerStatus.Queued || next == LegacyReturnPlanLedgerStatus.Failed;
            case LegacyReturnPlanLedgerStatus.Queued:
                switch (next)
                {
                    case LegacyReturnPlanLedgerStatus.Done:
                    case LegacyReturnPlanLedgerStatus.Failed:
                        return true;
                }
                return false;
            case LegacyReturnPlanLedgerStatus.Running:
                return next == LegacyReturnPlanLedgerStatus.Done || next == LegacyReturnPlanLedgerStatus.Failed;
            case LegacyReturnPlanLedgerStatus.Done:
                switch (next)
                {
                    case LegacyReturnPlanLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyReturnPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy return plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy return plan ledger"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public LegacyReturnPlanLedger Copy()
    {
        return new LegacyReturnPlanLedger(Code, Label, TenureMonths, Unit, Grade, Timezone, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
