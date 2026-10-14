namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip schedule is in its life cycle.</summary>
public enum PayslipScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A payslip schedule. Its code is unique within a tenant.</summary>
public sealed class PayslipSchedule(string code, string label, double cost, string location, double amount, PayslipScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public PayslipScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a payslip schedule in status planned.</summary>
    public static PayslipSchedule NewPayslipSchedule(string code, string label)
    {
        return new PayslipSchedule(code, label, 0.0, "", 0.0, PayslipScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipScheduleStatus next)
    {
        switch (Status)
        {
            case PayslipScheduleStatus.Planned:
                return next == PayslipScheduleStatus.Scheduled || next == PayslipScheduleStatus.Completed;
            case PayslipScheduleStatus.Scheduled:
                return next == PayslipScheduleStatus.Completed;
            case PayslipScheduleStatus.InProgress:
                switch (next)
                {
                    case PayslipScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case PayslipScheduleStatus.Completed:
                switch (next)
                {
                    case PayslipScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"payslip schedule"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Amount * Cost, 2);
    }

    public PayslipSchedule Copy()
    {
        return new PayslipSchedule(Code, Label, Cost, Location, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
