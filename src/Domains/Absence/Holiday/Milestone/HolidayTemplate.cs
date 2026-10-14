namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday template is in its life cycle.</summary>
public enum HolidayTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One holiday template, with what the planners need to act on it.</summary>
public sealed class HolidayTemplate(string code, string title, int priority, bool active, int retries, int capacity, HolidayTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public bool Active { get; set; } = active;
    public int Retries { get; set; } = retries;
    public int Capacity { get; set; } = capacity;
    public HolidayTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a holiday template in status open.</summary>
    public static HolidayTemplate NewHolidayTemplate(string code, string title)
    {
        return new HolidayTemplate(code, title, 0, false, 0, 0, HolidayTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayTemplateStatus next)
    {
        switch (Status)
        {
            case HolidayTemplateStatus.Open:
                switch (next)
                {
                    case HolidayTemplateStatus.Archived:
                        return true;
                }
                return false;
            case HolidayTemplateStatus.InReview:
                switch (next)
                {
                    case HolidayTemplateStatus.Resolved:
                    case HolidayTemplateStatus.Reopened:
                        return true;
                }
                return false;
            case HolidayTemplateStatus.Resolved:
                return next == HolidayTemplateStatus.Reopened || next == HolidayTemplateStatus.Archived;
            case HolidayTemplateStatus.Reopened:
                switch (next)
                {
                    case HolidayTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"holiday template"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority * (double)Capacity, 2);
    }

    public HolidayTemplate Copy()
    {
        return new HolidayTemplate(Code, Title, Priority, Active, Retries, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
