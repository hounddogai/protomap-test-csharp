namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual payslip allocation is in its life cycle.</summary>
public enum ManualPayslipAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual payslip allocation. Its code is unique within a tenant.</summary>
public sealed class ManualPayslipAllocation(string code, string name, double score, int graceDays, string country, string phone, int revision, double threshold, int tenureMonths, string department, ManualPayslipAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Department { get; set; } = department;
    public ManualPayslipAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a manual payslip allocation in status requested.</summary>
    public static ManualPayslipAllocation NewManualPayslipAllocation(string code, string name)
    {
        return new ManualPayslipAllocation(
            code,
            name,
            0.0,
            0,
            "",
            "",
            0,
            0.0,
            0,
            "",
            ManualPayslipAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPayslipAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPayslipAllocationStatus next)
    {
        switch (Status)
        {
            case ManualPayslipAllocationStatus.Requested:
                switch (next)
                {
                    case ManualPayslipAllocationStatus.Revoked:
                    case ManualPayslipAllocationStatus.Expired:
                        return true;
                }
                return false;
            case ManualPayslipAllocationStatus.Granted:
                switch (next)
                {
                    case ManualPayslipAllocationStatus.Expired:
                        return true;
                }
                return false;
            case ManualPayslipAllocationStatus.Revoked:
                switch (next)
                {
                    case ManualPayslipAllocationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualPayslipAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual payslip allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Threshold * (double)Revision, 2);
    }

    public ManualPayslipAllocation Copy()
    {
        return new ManualPayslipAllocation(
            Code,
            Name,
            Score,
            GraceDays,
            Country,
            Phone,
            Revision,
            Threshold,
            TenureMonths,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
