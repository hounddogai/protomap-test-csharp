namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual return plan ledger is in its life cycle.</summary>
public enum ManualReturnPlanLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual return plan ledger as the return plan team keeps it.</summary>
public sealed class ManualReturnPlanLedger(string code, string label, string notes, string summary, string reviewer, bool visible, string salaryBand, string checksum, double amount, ManualReturnPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public string Reviewer { get; set; } = reviewer;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public double Amount { get; set; } = amount;
    public ManualReturnPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a manual return plan ledger in status new.</summary>
    public static ManualReturnPlanLedger NewManualReturnPlanLedger(string code, string label)
    {
        return new ManualReturnPlanLedger(
            code,
            label,
            "",
            "",
            "",
            false,
            "",
            "",
            0.0,
            ManualReturnPlanLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualReturnPlanLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualReturnPlanLedgerStatus next)
    {
        switch (Status)
        {
            case ManualReturnPlanLedgerStatus.New:
                switch (next)
                {
                    case ManualReturnPlanLedgerStatus.Queued:
                    case ManualReturnPlanLedgerStatus.Running:
                        return true;
                }
                return false;
            case ManualReturnPlanLedgerStatus.Queued:
                switch (next)
                {
                    case ManualReturnPlanLedgerStatus.Running:
                    case ManualReturnPlanLedgerStatus.Failed:
                        return true;
                }
                return false;
            case ManualReturnPlanLedgerStatus.Running:
                return next == ManualReturnPlanLedgerStatus.Failed;
            case ManualReturnPlanLedgerStatus.Done:
                return next == ManualReturnPlanLedgerStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualReturnPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual return plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ManualReturnPlanLedger Copy()
    {
        return new ManualReturnPlanLedger(
            Code,
            Label,
            Notes,
            Summary,
            Reviewer,
            Visible,
            SalaryBand,
            Checksum,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
