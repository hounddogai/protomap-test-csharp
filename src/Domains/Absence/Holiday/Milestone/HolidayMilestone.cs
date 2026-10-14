namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday milestone is in its life cycle.</summary>
public enum HolidayMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A holiday milestone as the holiday team keeps it.</summary>
public sealed class HolidayMilestone(string code, string label, bool visible, string locale, string costCenter, int windowDays, HolidayMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public string CostCenter { get; set; } = costCenter;
    public int WindowDays { get; set; } = windowDays;
    public HolidayMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a holiday milestone in status new.</summary>
    public static HolidayMilestone NewHolidayMilestone(string code, string label)
    {
        return new HolidayMilestone(code, label, false, "", "", 0, HolidayMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayMilestoneStatus next)
    {
        switch (Status)
        {
            case HolidayMilestoneStatus.New:
                return next == HolidayMilestoneStatus.Failed;
            case HolidayMilestoneStatus.Queued:
                return next == HolidayMilestoneStatus.Running || next == HolidayMilestoneStatus.Failed;
            case HolidayMilestoneStatus.Running:
                return next == HolidayMilestoneStatus.Done || next == HolidayMilestoneStatus.Failed;
            case HolidayMilestoneStatus.Done:
                switch (next)
                {
                    case HolidayMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public HolidayMilestone Copy()
    {
        return new HolidayMilestone(Code, Label, Visible, Locale, CostCenter, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
