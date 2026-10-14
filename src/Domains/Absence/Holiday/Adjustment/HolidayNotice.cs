namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday notice is in its life cycle.</summary>
public enum HolidayNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a holiday notice and its current status.</summary>
public sealed class HolidayNotice(string code, string name, double cost, string skill, string region, string summary, string locale, HolidayNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public string Region { get; set; } = region;
    public string Summary { get; set; } = summary;
    public string Locale { get; set; } = locale;
    public HolidayNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a holiday notice in status new.</summary>
    public static HolidayNotice NewHolidayNotice(string code, string name)
    {
        return new HolidayNotice(code, name, 0.0, "", "", "", "", HolidayNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayNoticeStatus next)
    {
        switch (Status)
        {
            case HolidayNoticeStatus.New:
                switch (next)
                {
                    case HolidayNoticeStatus.Done:
                        return true;
                }
                return false;
            case HolidayNoticeStatus.Queued:
                switch (next)
                {
                    case HolidayNoticeStatus.Failed:
                        return true;
                }
                return false;
            case HolidayNoticeStatus.Running:
                switch (next)
                {
                    case HolidayNoticeStatus.Failed:
                        return true;
                }
                return false;
            case HolidayNoticeStatus.Done:
                switch (next)
                {
                    case HolidayNoticeStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public HolidayNotice Copy()
    {
        return new HolidayNotice(Code, Name, Cost, Skill, Region, Summary, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
