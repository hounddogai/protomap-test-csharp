namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip profile is in its life cycle.</summary>
public enum PayslipProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A payslip profile as the payslip team keeps it.</summary>
public sealed class PayslipProfile(string code, string title, double score, string segment, string employeeId, string phone, bool pinned, double cost, string name, string department, string checksum, PayslipProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Checksum { get; set; } = checksum;
    public PayslipProfileStatus Status { get; set; } = status;

    /// <summary>Starts a payslip profile in status proposed.</summary>
    public static PayslipProfile NewPayslipProfile(string code, string title)
    {
        return new PayslipProfile(
            code,
            title,
            0.0,
            "",
            "",
            "",
            false,
            0.0,
            "",
            "",
            "",
            PayslipProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipProfileStatus next)
    {
        switch (Status)
        {
            case PayslipProfileStatus.Proposed:
                switch (next)
                {
                    case PayslipProfileStatus.Approved:
                        return true;
                }
                return false;
            case PayslipProfileStatus.Approved:
                switch (next)
                {
                    case PayslipProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayslipProfileStatus.Rejected:
                return next == PayslipProfileStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayslipProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Cost + Score, 2);
    }

    public PayslipProfile Copy()
    {
        return new PayslipProfile(
            Code,
            Title,
            Score,
            Segment,
            EmployeeId,
            Phone,
            Pinned,
            Cost,
            Name,
            Department,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
