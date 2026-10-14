namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop plan is in its life cycle.</summary>
public enum LaptopPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a laptop plan and its current status.</summary>
public sealed class LaptopPlan(string code, string name, string country, int startsAt, string description, string source, bool archived, double floorValue, LaptopPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int StartsAt { get; set; } = startsAt;
    public string Description { get; set; } = description;
    public string Source { get; set; } = source;
    public bool Archived { get; set; } = archived;
    public double FloorValue { get; set; } = floorValue;
    public LaptopPlanStatus Status { get; set; } = status;

    /// <summary>Starts a laptop plan in status idle.</summary>
    public static LaptopPlan NewLaptopPlan(string code, string name)
    {
        return new LaptopPlan(code, name, "", 0, "", "", false, 0.0, LaptopPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopPlanStatus next)
    {
        switch (Status)
        {
            case LaptopPlanStatus.Idle:
                switch (next)
                {
                    case LaptopPlanStatus.Warming:
                    case LaptopPlanStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopPlanStatus.Warming:
                return next == LaptopPlanStatus.Ready;
            case LaptopPlanStatus.Ready:
                return next == LaptopPlanStatus.Draining || next == LaptopPlanStatus.Stopped;
            case LaptopPlanStatus.Draining:
                return next == LaptopPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LaptopPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"laptop plan"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue + (double)StartsAt, 2);
    }

    public LaptopPlan Copy()
    {
        return new LaptopPlan(Code, Name, Country, StartsAt, Description, Source, Archived, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
