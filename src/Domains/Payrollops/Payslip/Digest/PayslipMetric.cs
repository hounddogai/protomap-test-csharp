namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip metric is in its life cycle.</summary>
public enum PayslipMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A payslip metric as the payslip team keeps it.</summary>
public sealed class PayslipMetric(string code, string name, double cost, double maximum, double fte, double ceiling, string phone, string reviewer, string title, PayslipMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public double Maximum { get; set; } = maximum;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public string Reviewer { get; set; } = reviewer;
    public string Title { get; set; } = title;
    public PayslipMetricStatus Status { get; set; } = status;

    /// <summary>Starts a payslip metric in status created.</summary>
    public static PayslipMetric NewPayslipMetric(string code, string name)
    {
        return new PayslipMetric(code, name, 0.0, 0.0, 0.0, 0.0, "", "", "", PayslipMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipMetricStatus next)
    {
        switch (Status)
        {
            case PayslipMetricStatus.Created:
                switch (next)
                {
                    case PayslipMetricStatus.Published:
                    case PayslipMetricStatus.Retired:
                        return true;
                }
                return false;
            case PayslipMetricStatus.Validated:
                return next == PayslipMetricStatus.Retired;
            case PayslipMetricStatus.Published:
                switch (next)
                {
                    case PayslipMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte - Maximum, 2);
    }

    public PayslipMetric Copy()
    {
        return new PayslipMetric(Code, Name, Cost, Maximum, Fte, Ceiling, Phone, Reviewer, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
