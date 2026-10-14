namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy exception is in its life cycle.</summary>
public enum OccupancyExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a occupancy exception and its current status.</summary>
public sealed class OccupancyException(string code, string label, Dictionary<string, string> attributes, double score, int windowDays, string summary, bool approved, string timezone, OccupancyExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Score { get; set; } = score;
    public int WindowDays { get; set; } = windowDays;
    public string Summary { get; set; } = summary;
    public bool Approved { get; set; } = approved;
    public string Timezone { get; set; } = timezone;
    public OccupancyExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy exception in status planned.</summary>
    public static OccupancyException NewOccupancyException(string code, string label)
    {
        return new OccupancyException(
            code,
            label,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            false,
            "",
            OccupancyExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyExceptionStatus next)
    {
        switch (Status)
        {
            case OccupancyExceptionStatus.Planned:
                switch (next)
                {
                    case OccupancyExceptionStatus.InProgress:
                        return true;
                }
                return false;
            case OccupancyExceptionStatus.Scheduled:
                switch (next)
                {
                    case OccupancyExceptionStatus.InProgress:
                    case OccupancyExceptionStatus.Completed:
                        return true;
                }
                return false;
            case OccupancyExceptionStatus.InProgress:
                return next == OccupancyExceptionStatus.Cancelled;
            case OccupancyExceptionStatus.Completed:
                switch (next)
                {
                    case OccupancyExceptionStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedVolume()
    {
        return Math.Round(Score * (double)WindowDays, 2);
    }

    public OccupancyException Copy()
    {
        return new OccupancyException(
            Code,
            Label,
            Attributes,
            Score,
            WindowDays,
            Summary,
            Approved,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
