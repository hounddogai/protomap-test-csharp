namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient occupancy plan is in its life cycle.</summary>
public enum LenientOccupancyPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a lenient occupancy plan and its current status.</summary>
public sealed class LenientOccupancyPlan(string code, string title, double weight, double amount, string location, string costCenter, double floorValue, LenientOccupancyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public double Amount { get; set; } = amount;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public LenientOccupancyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a lenient occupancy plan in status requested.</summary>
    public static LenientOccupancyPlan NewLenientOccupancyPlan(string code, string title)
    {
        return new LenientOccupancyPlan(code, title, 0.0, 0.0, "", "", 0.0, LenientOccupancyPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientOccupancyPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientOccupancyPlanStatus next)
    {
        switch (Status)
        {
            case LenientOccupancyPlanStatus.Requested:
                switch (next)
                {
                    case LenientOccupancyPlanStatus.Expired:
                        return true;
                }
                return false;
            case LenientOccupancyPlanStatus.Granted:
                return next == LenientOccupancyPlanStatus.Revoked || next == LenientOccupancyPlanStatus.Expired;
            case LenientOccupancyPlanStatus.Revoked:
                switch (next)
                {
                    case LenientOccupancyPlanStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientOccupancyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient occupancy plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Weight + FloorValue, 2);
    }

    public LenientOccupancyPlan Copy()
    {
        return new LenientOccupancyPlan(Code, Title, Weight, Amount, Location, CostCenter, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
