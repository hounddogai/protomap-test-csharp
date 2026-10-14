namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered desk metric is in its life cycle.</summary>
public enum TieredDeskMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One tiered desk metric, with what the planners need to act on it.</summary>
public sealed class TieredDeskMetric(string code, string name, double floorValue, double ceiling, string managerId, double rating, string grade, string segment, TieredDeskMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public double Ceiling { get; set; } = ceiling;
    public string ManagerId { get; set; } = managerId;
    public double Rating { get; set; } = rating;
    public string Grade { get; set; } = grade;
    public string Segment { get; set; } = segment;
    public TieredDeskMetricStatus Status { get; set; } = status;

    /// <summary>Starts a tiered desk metric in status planned.</summary>
    public static TieredDeskMetric NewTieredDeskMetric(string code, string name)
    {
        return new TieredDeskMetric(code, name, 0.0, 0.0, "", 0.0, "", "", TieredDeskMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredDeskMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredDeskMetricStatus next)
    {
        switch (Status)
        {
            case TieredDeskMetricStatus.Planned:
                return next == TieredDeskMetricStatus.InProgress || next == TieredDeskMetricStatus.Completed;
            case TieredDeskMetricStatus.Scheduled:
                switch (next)
                {
                    case TieredDeskMetricStatus.Completed:
                    case TieredDeskMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredDeskMetricStatus.InProgress:
                switch (next)
                {
                    case TieredDeskMetricStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredDeskMetricStatus.Completed:
                switch (next)
                {
                    case TieredDeskMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredDeskMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered desk metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling * Rating, 2);
    }

    public TieredDeskMetric Copy()
    {
        return new TieredDeskMetric(Code, Name, FloorValue, Ceiling, ManagerId, Rating, Grade, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
