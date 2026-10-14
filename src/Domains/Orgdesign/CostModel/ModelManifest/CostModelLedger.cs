namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model ledger is in its life cycle.</summary>
public enum CostModelLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A cost model ledger as the cost model team keeps it.</summary>
public sealed class CostModelLedger(string code, string label, int durationMinutes, string salaryBand, double fte, double floorValue, int windowDays, int tenureMonths, string employeeId, CostModelLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public double FloorValue { get; set; } = floorValue;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string EmployeeId { get; set; } = employeeId;
    public CostModelLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a cost model ledger in status proposed.</summary>
    public static CostModelLedger NewCostModelLedger(string code, string label)
    {
        return new CostModelLedger(code, label, 0, "", 0.0, 0.0, 0, 0, "", CostModelLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelLedgerStatus next)
    {
        switch (Status)
        {
            case CostModelLedgerStatus.Proposed:
                return next == CostModelLedgerStatus.Approved;
            case CostModelLedgerStatus.Approved:
                return next == CostModelLedgerStatus.Withdrawn;
            case CostModelLedgerStatus.Rejected:
                return next == CostModelLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CostModelLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(FloorValue + (double)WindowDays, 2);
    }

    public CostModelLedger Copy()
    {
        return new CostModelLedger(
            Code,
            Label,
            DurationMinutes,
            SalaryBand,
            Fte,
            FloorValue,
            WindowDays,
            TenureMonths,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
