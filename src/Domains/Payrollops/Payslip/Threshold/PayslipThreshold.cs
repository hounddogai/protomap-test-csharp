namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip threshold is in its life cycle.</summary>
public enum PayslipThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a payslip threshold and its current status.</summary>
public sealed class PayslipThreshold(string code, string label, int graceDays, string contactEmail, string region, string checksum, PayslipThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public string Checksum { get; set; } = checksum;
    public PayslipThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a payslip threshold in status idle.</summary>
    public static PayslipThreshold NewPayslipThreshold(string code, string label)
    {
        return new PayslipThreshold(code, label, 0, "", "", "", PayslipThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipThresholdStatus next)
    {
        switch (Status)
        {
            case PayslipThresholdStatus.Idle:
                switch (next)
                {
                    case PayslipThresholdStatus.Draining:
                        return true;
                }
                return false;
            case PayslipThresholdStatus.Warming:
                return next == PayslipThresholdStatus.Stopped;
            case PayslipThresholdStatus.Ready:
                switch (next)
                {
                    case PayslipThresholdStatus.Draining:
                    case PayslipThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case PayslipThresholdStatus.Draining:
                return next == PayslipThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayslipThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public PayslipThreshold Copy()
    {
        return new PayslipThreshold(Code, Label, GraceDays, ContactEmail, Region, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
