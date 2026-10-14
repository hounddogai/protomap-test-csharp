namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding schedule is in its life cycle.</summary>
public enum OffboardingScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One offboarding schedule, with what the planners need to act on it.</summary>
public sealed class OffboardingSchedule(string code, string title, double budget, string category, string employeeId, string costCenter, string currency, OffboardingScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public string EmployeeId { get; set; } = employeeId;
    public string CostCenter { get; set; } = costCenter;
    public string Currency { get; set; } = currency;
    public OffboardingScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding schedule in status draft.</summary>
    public static OffboardingSchedule NewOffboardingSchedule(string code, string title)
    {
        return new OffboardingSchedule(code, title, 0.0, "", "", "", "", OffboardingScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingScheduleStatus next)
    {
        switch (Status)
        {
            case OffboardingScheduleStatus.Draft:
                return next == OffboardingScheduleStatus.Closed;
            case OffboardingScheduleStatus.Pending:
                switch (next)
                {
                    case OffboardingScheduleStatus.Suspended:
                    case OffboardingScheduleStatus.Closed:
                        return true;
                }
                return false;
            case OffboardingScheduleStatus.Active:
                return next == OffboardingScheduleStatus.Suspended || next == OffboardingScheduleStatus.Closed;
            case OffboardingScheduleStatus.Suspended:
                switch (next)
                {
                    case OffboardingScheduleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public OffboardingSchedule Copy()
    {
        return new OffboardingSchedule(Code, Title, Budget, Category, EmployeeId, CostCenter, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
