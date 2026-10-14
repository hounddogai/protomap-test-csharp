namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position plan is in its life cycle.</summary>
public enum LegacyPositionPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy position plan as the position team keeps it.</summary>
public sealed class LegacyPositionPlan(string code, string name, double weight, string country, int tenureMonths, string category, int capacity, string externalRef, string unit, LegacyPositionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Country { get; set; } = country;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Category { get; set; } = category;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public string Unit { get; set; } = unit;
    public LegacyPositionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position plan in status idle.</summary>
    public static LegacyPositionPlan NewLegacyPositionPlan(string code, string name)
    {
        return new LegacyPositionPlan(code, name, 0.0, "", 0, "", 0, "", "", LegacyPositionPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionPlanStatus next)
    {
        switch (Status)
        {
            case LegacyPositionPlanStatus.Idle:
                return next == LegacyPositionPlanStatus.Warming || next == LegacyPositionPlanStatus.Ready;
            case LegacyPositionPlanStatus.Warming:
                switch (next)
                {
                    case LegacyPositionPlanStatus.Draining:
                        return true;
                }
                return false;
            case LegacyPositionPlanStatus.Ready:
                switch (next)
                {
                    case LegacyPositionPlanStatus.Draining:
                        return true;
                }
                return false;
            case LegacyPositionPlanStatus.Draining:
                switch (next)
                {
                    case LegacyPositionPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPositionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy position plan"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)TenureMonths + (double)Capacity, 2);
    }

    public LegacyPositionPlan Copy()
    {
        return new LegacyPositionPlan(
            Code,
            Name,
            Weight,
            Country,
            TenureMonths,
            Category,
            Capacity,
            ExternalRef,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
