namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy plan is in its life cycle.</summary>
public enum OccupancyPlanStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One occupancy plan, with what the planners need to act on it.</summary>
public sealed class OccupancyPlan(string code, string name, double weight, double rate, string category, OccupancyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public string Category { get; set; } = category;
    public OccupancyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy plan in status open.</summary>
    public static OccupancyPlan NewOccupancyPlan(string code, string name)
    {
        return new OccupancyPlan(code, name, 0.0, 0.0, "", OccupancyPlanStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyPlanStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyPlanStatus next)
    {
        switch (Status)
        {
            case OccupancyPlanStatus.Open:
                return next == OccupancyPlanStatus.Archived;
            case OccupancyPlanStatus.InReview:
                switch (next)
                {
                    case OccupancyPlanStatus.Reopened:
                        return true;
                }
                return false;
            case OccupancyPlanStatus.Resolved:
                switch (next)
                {
                    case OccupancyPlanStatus.Reopened:
                    case OccupancyPlanStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyPlanStatus.Reopened:
                return next == OccupancyPlanStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OccupancyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Weight + Rate, 2);
    }

    public OccupancyPlan Copy()
    {
        return new OccupancyPlan(Code, Name, Weight, Rate, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
