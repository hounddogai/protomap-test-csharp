namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual rollup is in its life cycle.</summary>
public enum AccrualRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One accrual rollup, with what the planners need to act on it.</summary>
public sealed class AccrualRollup(string code, string name, string label, int graceDays, string category, double budget, string department, string checksum, string salaryBand, AccrualRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string Category { get; set; } = category;
    public double Budget { get; set; } = budget;
    public string Department { get; set; } = department;
    public string Checksum { get; set; } = checksum;
    public string SalaryBand { get; set; } = salaryBand;
    public AccrualRollupStatus Status { get; set; } = status;

    /// <summary>Starts a accrual rollup in status requested.</summary>
    public static AccrualRollup NewAccrualRollup(string code, string name)
    {
        return new AccrualRollup(code, name, "", 0, "", 0.0, "", "", "", AccrualRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualRollupStatus next)
    {
        switch (Status)
        {
            case AccrualRollupStatus.Requested:
                return next == AccrualRollupStatus.Granted;
            case AccrualRollupStatus.Granted:
                return next == AccrualRollupStatus.Revoked;
            case AccrualRollupStatus.Revoked:
                switch (next)
                {
                    case AccrualRollupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Budget - (double)GraceDays, 2);
    }

    public AccrualRollup Copy()
    {
        return new AccrualRollup(
            Code,
            Name,
            Label,
            GraceDays,
            Category,
            Budget,
            Department,
            Checksum,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
