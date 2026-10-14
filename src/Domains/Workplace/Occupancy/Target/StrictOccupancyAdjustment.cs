namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict occupancy adjustment is in its life cycle.</summary>
public enum StrictOccupancyAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A strict occupancy adjustment as the occupancy team keeps it.</summary>
public sealed class StrictOccupancyAdjustment(string code, string label, double minimum, int durationMinutes, string department, double amount, string channel, double budget, StrictOccupancyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public double Amount { get; set; } = amount;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public StrictOccupancyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict occupancy adjustment in status open.</summary>
    public static StrictOccupancyAdjustment NewStrictOccupancyAdjustment(string code, string label)
    {
        return new StrictOccupancyAdjustment(
            code,
            label,
            0.0,
            0,
            "",
            0.0,
            "",
            0.0,
            StrictOccupancyAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOccupancyAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOccupancyAdjustmentStatus next)
    {
        switch (Status)
        {
            case StrictOccupancyAdjustmentStatus.Open:
                return next == StrictOccupancyAdjustmentStatus.InReview || next == StrictOccupancyAdjustmentStatus.Resolved;
            case StrictOccupancyAdjustmentStatus.InReview:
                switch (next)
                {
                    case StrictOccupancyAdjustmentStatus.Reopened:
                        return true;
                }
                return false;
            case StrictOccupancyAdjustmentStatus.Resolved:
                switch (next)
                {
                    case StrictOccupancyAdjustmentStatus.Reopened:
                    case StrictOccupancyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case StrictOccupancyAdjustmentStatus.Reopened:
                switch (next)
                {
                    case StrictOccupancyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictOccupancyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict occupancy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict occupancy adjustment"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum - Budget, 2);
    }

    public StrictOccupancyAdjustment Copy()
    {
        return new StrictOccupancyAdjustment(
            Code,
            Label,
            Minimum,
            DurationMinutes,
            Department,
            Amount,
            Channel,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
