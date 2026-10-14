namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy target is in its life cycle.</summary>
public enum OccupancyTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A occupancy target. Its code is unique within a tenant.</summary>
public sealed class OccupancyTarget(string code, string name, double fte, double hours, string contactEmail, string jobCode, string location, double ratio, OccupancyTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public double Ratio { get; set; } = ratio;
    public OccupancyTargetStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy target in status planned.</summary>
    public static OccupancyTarget NewOccupancyTarget(string code, string name)
    {
        return new OccupancyTarget(code, name, 0.0, 0.0, "", "", "", 0.0, OccupancyTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyTargetStatus next)
    {
        switch (Status)
        {
            case OccupancyTargetStatus.Planned:
                switch (next)
                {
                    case OccupancyTargetStatus.Completed:
                    case OccupancyTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case OccupancyTargetStatus.Scheduled:
                switch (next)
                {
                    case OccupancyTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case OccupancyTargetStatus.InProgress:
                switch (next)
                {
                    case OccupancyTargetStatus.Completed:
                        return true;
                }
                return false;
            case OccupancyTargetStatus.Completed:
                return next == OccupancyTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OccupancyTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"occupancy target"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Fte + Ratio, 2);
    }

    public OccupancyTarget Copy()
    {
        return new OccupancyTarget(Code, Name, Fte, Hours, ContactEmail, JobCode, Location, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
