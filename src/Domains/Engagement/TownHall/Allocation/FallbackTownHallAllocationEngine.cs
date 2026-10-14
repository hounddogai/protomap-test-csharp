namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback town hall allocation calculation.</summary>
public interface IFallbackTownHallAllocationAdjuster
{
    string Describe();
    double Adjust(FallbackTownHallAllocation item, double valueValue);
}

public sealed record TitleFallbackTownHallAllocationAdjuster(Dictionary<string, double> Factors) : IFallbackTownHallAllocationAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackTownHallAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackTownHallAllocationAdjuster(double Step) : IFallbackTownHallAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackTownHallAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFallbackTownHallAllocationAdjuster(double Minimum) : IFallbackTownHallAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackTownHallAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackTownHallAllocationAdjuster(double Cap) : IFallbackTownHallAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackTownHallAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFallbackTownHallAllocationAdjuster(double Percent) : IFallbackTownHallAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackTownHallAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackTownHallAllocationAttemptsOffsetAdjuster(double Weight) : IFallbackTownHallAllocationAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(FallbackTownHallAllocation item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

/// <summary>Runs the fallback town hall allocation steps in order and keeps a trace of each.</summary>
public sealed class FallbackTownHallAllocationEngine(List<IFallbackTownHallAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackTownHallAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackTownHallAllocationEngine DefaultFallbackTownHallAllocationEngine()
    {
        return new FallbackTownHallAllocationEngine(
            new List<IFallbackTownHallAllocationAdjuster>
            {
                new TitleFallbackTownHallAllocationAdjuster(new Dictionary<string, double> { ["online"] = 1.06 }),
                new RoundingFallbackTownHallAllocationAdjuster(0.5),
                new MinimumFallbackTownHallAllocationAdjuster(24.4),
                new CapFallbackTownHallAllocationAdjuster(48598.0),
                new PercentFallbackTownHallAllocationAdjuster(-3.6),
                new FallbackTownHallAllocationAttemptsOffsetAdjuster(0.6),
            },
            new List<string>(),
            3.9,
            89347.3);
    }

    public double Run(FallbackTownHallAllocation item)
    {
        var current = item.NetLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackTownHallAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
