namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan threshold is in its life cycle.</summary>
public enum FloorPlanThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A floor plan threshold as the floor plan team keeps it.</summary>
public sealed class FloorPlanThreshold(string code, string title, int graceDays, string location, int endsAt, double minimum, double ratio, int startsAt, bool archived, string salaryBand, FloorPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string Location { get; set; } = location;
    public int EndsAt { get; set; } = endsAt;
    public double Minimum { get; set; } = minimum;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public string SalaryBand { get; set; } = salaryBand;
    public FloorPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan threshold in status new.</summary>
    public static FloorPlanThreshold NewFloorPlanThreshold(string code, string title)
    {
        return new FloorPlanThreshold(code, title, 0, "", 0, 0.0, 0.0, 0, false, "", FloorPlanThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanThresholdStatus next)
    {
        switch (Status)
        {
            case FloorPlanThresholdStatus.New:
                switch (next)
                {
                    case FloorPlanThresholdStatus.Queued:
                        return true;
                }
                return false;
            case FloorPlanThresholdStatus.Queued:
                return next == FloorPlanThresholdStatus.Done;
            case FloorPlanThresholdStatus.Running:
                return next == FloorPlanThresholdStatus.Done;
            case FloorPlanThresholdStatus.Done:
                switch (next)
                {
                    case FloorPlanThresholdStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt + (double)StartsAt, 2);
    }

    public FloorPlanThreshold Copy()
    {
        return new FloorPlanThreshold(
            Code,
            Title,
            GraceDays,
            Location,
            EndsAt,
            Minimum,
            Ratio,
            StartsAt,
            Archived,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
