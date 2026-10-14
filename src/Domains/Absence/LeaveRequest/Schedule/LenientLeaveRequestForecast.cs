namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient leave request forecast is in its life cycle.</summary>
public enum LenientLeaveRequestForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient leave request forecast and its current status.</summary>
public sealed class LenientLeaveRequestForecast(string code, string label, string location, string category, double minimum, double floorValue, List<string> tags, LenientLeaveRequestForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string Category { get; set; } = category;
    public double Minimum { get; set; } = minimum;
    public double FloorValue { get; set; } = floorValue;
    public List<string> Tags { get; set; } = tags;
    public LenientLeaveRequestForecastStatus Status { get; set; } = status;

    /// <summary>Starts a lenient leave request forecast in status planned.</summary>
    public static LenientLeaveRequestForecast NewLenientLeaveRequestForecast(string code, string label)
    {
        return new LenientLeaveRequestForecast(
            code,
            label,
            "",
            "",
            0.0,
            0.0,
            new List<string>(),
            LenientLeaveRequestForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLeaveRequestForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLeaveRequestForecastStatus next)
    {
        switch (Status)
        {
            case LenientLeaveRequestForecastStatus.Planned:
                switch (next)
                {
                    case LenientLeaveRequestForecastStatus.Completed:
                    case LenientLeaveRequestForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientLeaveRequestForecastStatus.Scheduled:
                switch (next)
                {
                    case LenientLeaveRequestForecastStatus.InProgress:
                    case LenientLeaveRequestForecastStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientLeaveRequestForecastStatus.InProgress:
                return next == LenientLeaveRequestForecastStatus.Completed || next == LenientLeaveRequestForecastStatus.Cancelled;
            case LenientLeaveRequestForecastStatus.Completed:
                switch (next)
                {
                    case LenientLeaveRequestForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLeaveRequestForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient leave request forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue * Minimum, 2);
    }

    public LenientLeaveRequestForecast Copy()
    {
        return new LenientLeaveRequestForecast(Code, Label, Location, Category, Minimum, FloorValue, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
