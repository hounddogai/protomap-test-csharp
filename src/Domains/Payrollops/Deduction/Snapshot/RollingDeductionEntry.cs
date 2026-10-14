namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling deduction entry is in its life cycle.</summary>
public enum RollingDeductionEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rolling deduction entry and its current status.</summary>
public sealed class RollingDeductionEntry(string code, string title, int tenureMonths, string contactEmail, string department, double rate, double ratio, string country, double cost, string notes, RollingDeductionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ContactEmail { get; set; } = contactEmail;
    public string Department { get; set; } = department;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public RollingDeductionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling deduction entry in status idle.</summary>
    public static RollingDeductionEntry NewRollingDeductionEntry(string code, string title)
    {
        return new RollingDeductionEntry(
            code,
            title,
            0,
            "",
            "",
            0.0,
            0.0,
            "",
            0.0,
            "",
            RollingDeductionEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingDeductionEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingDeductionEntryStatus next)
    {
        switch (Status)
        {
            case RollingDeductionEntryStatus.Idle:
                switch (next)
                {
                    case RollingDeductionEntryStatus.Warming:
                    case RollingDeductionEntryStatus.Stopped:
                        return true;
                }
                return false;
            case RollingDeductionEntryStatus.Warming:
                switch (next)
                {
                    case RollingDeductionEntryStatus.Ready:
                    case RollingDeductionEntryStatus.Stopped:
                        return true;
                }
                return false;
            case RollingDeductionEntryStatus.Ready:
                switch (next)
                {
                    case RollingDeductionEntryStatus.Draining:
                        return true;
                }
                return false;
            case RollingDeductionEntryStatus.Draining:
                switch (next)
                {
                    case RollingDeductionEntryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingDeductionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling deduction entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling deduction entry"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rate - (double)TenureMonths, 2);
    }

    public RollingDeductionEntry Copy()
    {
        return new RollingDeductionEntry(
            Code,
            Title,
            TenureMonths,
            ContactEmail,
            Department,
            Rate,
            Ratio,
            Country,
            Cost,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
