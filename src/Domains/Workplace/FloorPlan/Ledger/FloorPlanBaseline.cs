namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan baseline is in its life cycle.</summary>
public enum FloorPlanBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a floor plan baseline and its current status.</summary>
public sealed class FloorPlanBaseline(string code, string name, double score, int startsAt, double minimum, string salaryBand, double weight, double discount, double rating, FloorPlanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int StartsAt { get; set; } = startsAt;
    public double Minimum { get; set; } = minimum;
    public string SalaryBand { get; set; } = salaryBand;
    public double Weight { get; set; } = weight;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public FloorPlanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan baseline in status proposed.</summary>
    public static FloorPlanBaseline NewFloorPlanBaseline(string code, string name)
    {
        return new FloorPlanBaseline(code, name, 0.0, 0, 0.0, "", 0.0, 0.0, 0.0, FloorPlanBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanBaselineStatus next)
    {
        switch (Status)
        {
            case FloorPlanBaselineStatus.Proposed:
                switch (next)
                {
                    case FloorPlanBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case FloorPlanBaselineStatus.Approved:
                switch (next)
                {
                    case FloorPlanBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case FloorPlanBaselineStatus.Rejected:
                return next == FloorPlanBaselineStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FloorPlanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Score + Discount, 2);
    }

    public FloorPlanBaseline Copy()
    {
        return new FloorPlanBaseline(
            Code,
            Name,
            Score,
            StartsAt,
            Minimum,
            SalaryBand,
            Weight,
            Discount,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
