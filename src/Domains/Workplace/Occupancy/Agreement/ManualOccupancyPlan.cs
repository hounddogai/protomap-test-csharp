namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual occupancy plan is in its life cycle.</summary>
public enum ManualOccupancyPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a manual occupancy plan and its current status.</summary>
public sealed class ManualOccupancyPlan(string code, string name, string segment, int retries, double weight, string skill, int endsAt, double rating, int attempts, ManualOccupancyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int Retries { get; set; } = retries;
    public double Weight { get; set; } = weight;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public ManualOccupancyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a manual occupancy plan in status new.</summary>
    public static ManualOccupancyPlan NewManualOccupancyPlan(string code, string name)
    {
        return new ManualOccupancyPlan(code, name, "", 0, 0.0, "", 0, 0.0, 0, ManualOccupancyPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualOccupancyPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualOccupancyPlanStatus next)
    {
        switch (Status)
        {
            case ManualOccupancyPlanStatus.New:
                return next == ManualOccupancyPlanStatus.Queued || next == ManualOccupancyPlanStatus.Done;
            case ManualOccupancyPlanStatus.Queued:
                switch (next)
                {
                    case ManualOccupancyPlanStatus.Running:
                    case ManualOccupancyPlanStatus.Done:
                        return true;
                }
                return false;
            case ManualOccupancyPlanStatus.Running:
                switch (next)
                {
                    case ManualOccupancyPlanStatus.Done:
                    case ManualOccupancyPlanStatus.Failed:
                        return true;
                }
                return false;
            case ManualOccupancyPlanStatus.Done:
                return next == ManualOccupancyPlanStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualOccupancyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual occupancy plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Rating * (double)Retries, 2);
    }

    public ManualOccupancyPlan Copy()
    {
        return new ManualOccupancyPlan(
            Code,
            Name,
            Segment,
            Retries,
            Weight,
            Skill,
            EndsAt,
            Rating,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
