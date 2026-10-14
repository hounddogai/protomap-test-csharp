namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday request is in its life cycle.</summary>
public enum HolidayRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One holiday request, with what the planners need to act on it.</summary>
public sealed class HolidayRequest(string code, string name, double fte, double weight, int priority, string jobCode, int revision, double ratio, double margin, double ceiling, HolidayRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public string JobCode { get; set; } = jobCode;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public double Margin { get; set; } = margin;
    public double Ceiling { get; set; } = ceiling;
    public HolidayRequestStatus Status { get; set; } = status;

    /// <summary>Starts a holiday request in status planned.</summary>
    public static HolidayRequest NewHolidayRequest(string code, string name)
    {
        return new HolidayRequest(code, name, 0.0, 0.0, 0, "", 0, 0.0, 0.0, 0.0, HolidayRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayRequestStatus next)
    {
        switch (Status)
        {
            case HolidayRequestStatus.Planned:
                return next == HolidayRequestStatus.Cancelled;
            case HolidayRequestStatus.Scheduled:
                return next == HolidayRequestStatus.InProgress;
            case HolidayRequestStatus.InProgress:
                switch (next)
                {
                    case HolidayRequestStatus.Completed:
                        return true;
                }
                return false;
            case HolidayRequestStatus.Completed:
                return next == HolidayRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(HolidayRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"holiday request"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority + (double)Revision, 2);
    }

    public HolidayRequest Copy()
    {
        return new HolidayRequest(
            Code,
            Name,
            Fte,
            Weight,
            Priority,
            JobCode,
            Revision,
            Ratio,
            Margin,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
