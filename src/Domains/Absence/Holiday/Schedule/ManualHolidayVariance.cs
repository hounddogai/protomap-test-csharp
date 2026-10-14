namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual holiday variance is in its life cycle.</summary>
public enum ManualHolidayVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual holiday variance as the holiday team keeps it.</summary>
public sealed class ManualHolidayVariance(string code, string name, int sequence, double hours, double minimum, string title, int windowDays, string jobCode, ManualHolidayVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public double Minimum { get; set; } = minimum;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string JobCode { get; set; } = jobCode;
    public ManualHolidayVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a manual holiday variance in status planned.</summary>
    public static ManualHolidayVariance NewManualHolidayVariance(string code, string name)
    {
        return new ManualHolidayVariance(code, name, 0, 0.0, 0.0, "", 0, "", ManualHolidayVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualHolidayVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualHolidayVarianceStatus next)
    {
        switch (Status)
        {
            case ManualHolidayVarianceStatus.Planned:
                switch (next)
                {
                    case ManualHolidayVarianceStatus.Scheduled:
                    case ManualHolidayVarianceStatus.Completed:
                        return true;
                }
                return false;
            case ManualHolidayVarianceStatus.Scheduled:
                return next == ManualHolidayVarianceStatus.Completed || next == ManualHolidayVarianceStatus.Cancelled;
            case ManualHolidayVarianceStatus.InProgress:
                return next == ManualHolidayVarianceStatus.Cancelled;
            case ManualHolidayVarianceStatus.Completed:
                return next == ManualHolidayVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualHolidayVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual holiday variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Hours - (double)Sequence, 2);
    }

    public ManualHolidayVariance Copy()
    {
        return new ManualHolidayVariance(Code, Name, Sequence, Hours, Minimum, Title, WindowDays, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
