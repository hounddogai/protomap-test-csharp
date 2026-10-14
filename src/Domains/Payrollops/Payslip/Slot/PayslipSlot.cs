namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip slot is in its life cycle.</summary>
public enum PayslipSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip slot. Its code is unique within a tenant.</summary>
public sealed class PayslipSlot(string code, string title, double ceiling, string department, double margin, double maximum, string contactEmail, string source, double rate, string badge, int sequence, PayslipSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public PayslipSlotStatus Status { get; set; } = status;

    /// <summary>Starts a payslip slot in status requested.</summary>
    public static PayslipSlot NewPayslipSlot(string code, string title)
    {
        return new PayslipSlot(code, title, 0.0, "", 0.0, 0.0, "", "", 0.0, "", 0, PayslipSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipSlotStatus next)
    {
        switch (Status)
        {
            case PayslipSlotStatus.Requested:
                return next == PayslipSlotStatus.Expired;
            case PayslipSlotStatus.Granted:
                switch (next)
                {
                    case PayslipSlotStatus.Expired:
                        return true;
                }
                return false;
            case PayslipSlotStatus.Revoked:
                switch (next)
                {
                    case PayslipSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"payslip slot"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence + Maximum, 2);
    }

    public PayslipSlot Copy()
    {
        return new PayslipSlot(
            Code,
            Title,
            Ceiling,
            Department,
            Margin,
            Maximum,
            ContactEmail,
            Source,
            Rate,
            Badge,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
