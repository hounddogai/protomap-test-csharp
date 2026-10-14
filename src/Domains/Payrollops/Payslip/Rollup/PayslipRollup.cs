namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip rollup is in its life cycle.</summary>
public enum PayslipRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One payslip rollup, with what the planners need to act on it.</summary>
public sealed class PayslipRollup(string code, string name, int retries, double maximum, double discount, int tenureMonths, string description, string unit, string channel, PayslipRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public string Unit { get; set; } = unit;
    public string Channel { get; set; } = channel;
    public PayslipRollupStatus Status { get; set; } = status;

    /// <summary>Starts a payslip rollup in status idle.</summary>
    public static PayslipRollup NewPayslipRollup(string code, string name)
    {
        return new PayslipRollup(code, name, 0, 0.0, 0.0, 0, "", "", "", PayslipRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipRollupStatus next)
    {
        switch (Status)
        {
            case PayslipRollupStatus.Idle:
                return next == PayslipRollupStatus.Stopped;
            case PayslipRollupStatus.Warming:
                return next == PayslipRollupStatus.Ready || next == PayslipRollupStatus.Stopped;
            case PayslipRollupStatus.Ready:
                switch (next)
                {
                    case PayslipRollupStatus.Draining:
                        return true;
                }
                return false;
            case PayslipRollupStatus.Draining:
                switch (next)
                {
                    case PayslipRollupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Maximum - (double)Retries, 2);
    }

    public PayslipRollup Copy()
    {
        return new PayslipRollup(
            Code,
            Name,
            Retries,
            Maximum,
            Discount,
            TenureMonths,
            Description,
            Unit,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
