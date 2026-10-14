namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan profile calculation.</summary>
public interface IFloorPlanProfileFactor
{
    string Describe();
    double Adjust(FloorPlanProfile item, double valueValue);
}

public sealed record LocaleFloorPlanProfileAdjuster(Dictionary<string, double> Factors) : IFloorPlanProfileFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FloorPlanProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FloorPlanProfileWeightOffsetAdjuster(double Weight) : IFloorPlanProfileFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(FloorPlanProfile item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumFloorPlanProfileAdjuster(double Minimum) : IFloorPlanProfileFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFloorPlanProfileAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanProfileFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFloorPlanProfileAdjuster(double Step) : IFloorPlanProfileFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFloorPlanProfileAdjuster(double Cap) : IFloorPlanProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the floor plan profile steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanProfileEngine(List<IFloorPlanProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanProfileEngine DefaultFloorPlanProfileEngine(Registry flags)
    {
        return new FloorPlanProfileEngine(
            new List<IFloorPlanProfileFactor>
            {
                new LocaleFloorPlanProfileAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.75, ["online"] = 0.88, ["retail"] = 0.99 }),
                new FloorPlanProfileWeightOffsetAdjuster(1.85),
                new MinimumFloorPlanProfileAdjuster(33.0),
                new FlaggedFloorPlanProfileAdjuster(flags, "floor-plan-profile-primary", -5.2),
                new RoundingFloorPlanProfileAdjuster(5.0),
                new CapFloorPlanProfileAdjuster(40840.0),
            },
            new List<string>(),
            3.9,
            96126.0);
    }

    public double Price(FloorPlanProfile item)
    {
        var current = item.WeightedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FloorPlanProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
