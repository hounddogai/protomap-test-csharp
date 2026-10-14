namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip entry is in its life cycle.</summary>
public enum PayslipEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A payslip entry as the payslip team keeps it.</summary>
public sealed class PayslipEntry(string code, string title, string jobCode, bool active, string location, double score, double ceiling, int retries, string timezone, string label, PayslipEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public double Score { get; set; } = score;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public string Timezone { get; set; } = timezone;
    public string Label { get; set; } = label;
    public PayslipEntryStatus Status { get; set; } = status;

    /// <summary>Starts a payslip entry in status proposed.</summary>
    public static PayslipEntry NewPayslipEntry(string code, string title)
    {
        return new PayslipEntry(code, title, "", false, "", 0.0, 0.0, 0, "", "", PayslipEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipEntryStatus next)
    {
        switch (Status)
        {
            case PayslipEntryStatus.Proposed:
                switch (next)
                {
                    case PayslipEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayslipEntryStatus.Approved:
                return next == PayslipEntryStatus.Rejected || next == PayslipEntryStatus.Withdrawn;
            case PayslipEntryStatus.Rejected:
                switch (next)
                {
                    case PayslipEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Ceiling * (double)Retries, 2);
    }

    public PayslipEntry Copy()
    {
        return new PayslipEntry(
            Code,
            Title,
            JobCode,
            Active,
            Location,
            Score,
            Ceiling,
            Retries,
            Timezone,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
