namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment ledger is in its life cycle.</summary>
public enum AssignmentLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A assignment ledger as the assignment team keeps it.</summary>
public sealed class AssignmentLedger(string code, string label, double rate, string salaryBand, string location, string locale, string grade, string region, string skill, string employeeId, AssignmentLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public string Locale { get; set; } = locale;
    public string Grade { get; set; } = grade;
    public string Region { get; set; } = region;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public AssignmentLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a assignment ledger in status new.</summary>
    public static AssignmentLedger NewAssignmentLedger(string code, string label)
    {
        return new AssignmentLedger(code, label, 0.0, "", "", "", "", "", "", "", AssignmentLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentLedgerStatus next)
    {
        switch (Status)
        {
            case AssignmentLedgerStatus.New:
                return next == AssignmentLedgerStatus.Failed;
            case AssignmentLedgerStatus.Queued:
                return next == AssignmentLedgerStatus.Failed;
            case AssignmentLedgerStatus.Running:
                return next == AssignmentLedgerStatus.Failed;
            case AssignmentLedgerStatus.Done:
                return next == AssignmentLedgerStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssignmentLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public AssignmentLedger Copy()
    {
        return new AssignmentLedger(
            Code,
            Label,
            Rate,
            SalaryBand,
            Location,
            Locale,
            Grade,
            Region,
            Skill,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
