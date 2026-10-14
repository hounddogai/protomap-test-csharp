namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip group is in its life cycle.</summary>
public enum PayslipGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One payslip group, with what the planners need to act on it.</summary>
public sealed class PayslipGroup(string code, string label, int tenureMonths, string checksum, double floorValue, int startsAt, string contactEmail, PayslipGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Checksum { get; set; } = checksum;
    public double FloorValue { get; set; } = floorValue;
    public int StartsAt { get; set; } = startsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public PayslipGroupStatus Status { get; set; } = status;

    /// <summary>Starts a payslip group in status new.</summary>
    public static PayslipGroup NewPayslipGroup(string code, string label)
    {
        return new PayslipGroup(code, label, 0, "", 0.0, 0, "", PayslipGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipGroupStatus next)
    {
        switch (Status)
        {
            case PayslipGroupStatus.New:
                return next == PayslipGroupStatus.Running || next == PayslipGroupStatus.Done;
            case PayslipGroupStatus.Queued:
                return next == PayslipGroupStatus.Running || next == PayslipGroupStatus.Done;
            case PayslipGroupStatus.Running:
                return next == PayslipGroupStatus.Done || next == PayslipGroupStatus.Failed;
            case PayslipGroupStatus.Done:
                switch (next)
                {
                    case PayslipGroupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)TenureMonths + (double)StartsAt, 2);
    }

    public PayslipGroup Copy()
    {
        return new PayslipGroup(Code, Label, TenureMonths, Checksum, FloorValue, StartsAt, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
