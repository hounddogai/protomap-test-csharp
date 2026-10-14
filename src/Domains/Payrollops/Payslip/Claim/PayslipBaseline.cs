namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip baseline is in its life cycle.</summary>
public enum PayslipBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A payslip baseline. Its code is unique within a tenant.</summary>
public sealed class PayslipBaseline(string code, string title, double hours, int sequence, string externalRef, int updatedAt, string locale, double ratio, int quantity, PayslipBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int Sequence { get; set; } = sequence;
    public string ExternalRef { get; set; } = externalRef;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Locale { get; set; } = locale;
    public double Ratio { get; set; } = ratio;
    public int Quantity { get; set; } = quantity;
    public PayslipBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a payslip baseline in status planned.</summary>
    public static PayslipBaseline NewPayslipBaseline(string code, string title)
    {
        return new PayslipBaseline(code, title, 0.0, 0, "", 0, "", 0.0, 0, PayslipBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipBaselineStatus next)
    {
        switch (Status)
        {
            case PayslipBaselineStatus.Planned:
                switch (next)
                {
                    case PayslipBaselineStatus.InProgress:
                        return true;
                }
                return false;
            case PayslipBaselineStatus.Scheduled:
                switch (next)
                {
                    case PayslipBaselineStatus.InProgress:
                        return true;
                }
                return false;
            case PayslipBaselineStatus.InProgress:
                switch (next)
                {
                    case PayslipBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case PayslipBaselineStatus.Completed:
                switch (next)
                {
                    case PayslipBaselineStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ratio * Hours, 2);
    }

    public PayslipBaseline Copy()
    {
        return new PayslipBaseline(
            Code,
            Title,
            Hours,
            Sequence,
            ExternalRef,
            UpdatedAt,
            Locale,
            Ratio,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
