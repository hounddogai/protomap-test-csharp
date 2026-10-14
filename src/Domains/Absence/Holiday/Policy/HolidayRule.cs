namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday rule is in its life cycle.</summary>
public enum HolidayRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A holiday rule as the holiday team keeps it.</summary>
public sealed class HolidayRule(string code, string title, string salaryBand, double maximum, string grade, string currency, string timezone, HolidayRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public string Currency { get; set; } = currency;
    public string Timezone { get; set; } = timezone;
    public HolidayRuleStatus Status { get; set; } = status;

    /// <summary>Starts a holiday rule in status new.</summary>
    public static HolidayRule NewHolidayRule(string code, string title)
    {
        return new HolidayRule(code, title, "", 0.0, "", "", "", HolidayRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayRuleStatus next)
    {
        switch (Status)
        {
            case HolidayRuleStatus.New:
                switch (next)
                {
                    case HolidayRuleStatus.Running:
                    case HolidayRuleStatus.Done:
                        return true;
                }
                return false;
            case HolidayRuleStatus.Queued:
                return next == HolidayRuleStatus.Running || next == HolidayRuleStatus.Failed;
            case HolidayRuleStatus.Running:
                switch (next)
                {
                    case HolidayRuleStatus.Done:
                        return true;
                }
                return false;
            case HolidayRuleStatus.Done:
                return next == HolidayRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(HolidayRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public HolidayRule Copy()
    {
        return new HolidayRule(Code, Title, SalaryBand, Maximum, Grade, Currency, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
