namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal payslip slot is in its life cycle.</summary>
public enum SeasonalPayslipSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One seasonal payslip slot, with what the planners need to act on it.</summary>
public sealed class SeasonalPayslipSlot(string code, string name, string team, string currency, string unit, double hours, bool active, int tenureMonths, SeasonalPayslipSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public int TenureMonths { get; set; } = tenureMonths;
    public SeasonalPayslipSlotStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal payslip slot in status open.</summary>
    public static SeasonalPayslipSlot NewSeasonalPayslipSlot(string code, string name)
    {
        return new SeasonalPayslipSlot(code, name, "", "", "", 0.0, false, 0, SeasonalPayslipSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayslipSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayslipSlotStatus next)
    {
        switch (Status)
        {
            case SeasonalPayslipSlotStatus.Open:
                return next == SeasonalPayslipSlotStatus.Resolved;
            case SeasonalPayslipSlotStatus.InReview:
                return next == SeasonalPayslipSlotStatus.Resolved || next == SeasonalPayslipSlotStatus.Archived;
            case SeasonalPayslipSlotStatus.Resolved:
                switch (next)
                {
                    case SeasonalPayslipSlotStatus.Reopened:
                        return true;
                }
                return false;
            case SeasonalPayslipSlotStatus.Reopened:
                switch (next)
                {
                    case SeasonalPayslipSlotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPayslipSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal payslip slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round(Hours + (double)TenureMonths, 2);
    }

    public SeasonalPayslipSlot Copy()
    {
        return new SeasonalPayslipSlot(Code, Name, Team, Currency, Unit, Hours, Active, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
