namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional holiday cycle is in its life cycle.</summary>
public enum RegionalHolidayCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a regional holiday cycle and its current status.</summary>
public sealed class RegionalHolidayCycle(string code, string name, double budget, int priority, string reviewer, string salaryBand, string department, string jobCode, string contactEmail, RegionalHolidayCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public string SalaryBand { get; set; } = salaryBand;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public RegionalHolidayCycleStatus Status { get; set; } = status;

    /// <summary>Starts a regional holiday cycle in status requested.</summary>
    public static RegionalHolidayCycle NewRegionalHolidayCycle(string code, string name)
    {
        return new RegionalHolidayCycle(
            code,
            name,
            0.0,
            0,
            "",
            "",
            "",
            "",
            "",
            RegionalHolidayCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalHolidayCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalHolidayCycleStatus next)
    {
        switch (Status)
        {
            case RegionalHolidayCycleStatus.Requested:
                switch (next)
                {
                    case RegionalHolidayCycleStatus.Granted:
                    case RegionalHolidayCycleStatus.Revoked:
                        return true;
                }
                return false;
            case RegionalHolidayCycleStatus.Granted:
                return next == RegionalHolidayCycleStatus.Revoked || next == RegionalHolidayCycleStatus.Expired;
            case RegionalHolidayCycleStatus.Revoked:
                return next == RegionalHolidayCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalHolidayCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional holiday cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Priority - Budget, 2);
    }

    public RegionalHolidayCycle Copy()
    {
        return new RegionalHolidayCycle(
            Code,
            Name,
            Budget,
            Priority,
            Reviewer,
            SalaryBand,
            Department,
            JobCode,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
