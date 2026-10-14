namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default occupancy estimate calculation.</summary>
public interface IDefaultOccupancyEstimateStep
{
    string Describe();
    double Adjust(DefaultOccupancyEstimate item, double valueValue);
}

public sealed record CapDefaultOccupancyEstimateAdjuster(double Cap) : IDefaultOccupancyEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultOccupancyEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelDefaultOccupancyEstimateAdjuster(Dictionary<string, double> Factors) : IDefaultOccupancyEstimateStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultOccupancyEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultOccupancyEstimateAdjuster(List<double> Bounds, List<double> Rates) : IDefaultOccupancyEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultOccupancyEstimate item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record RoundingDefaultOccupancyEstimateAdjuster(double Step) : IDefaultOccupancyEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultOccupancyEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default occupancy estimate steps in order and keeps a trace of each.</summary>
public sealed class DefaultOccupancyEstimateEngine(List<IDefaultOccupancyEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultOccupancyEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultOccupancyEstimateEngine DefaultDefaultOccupancyEstimateEngine()
    {
        return new DefaultOccupancyEstimateEngine(
            new List<IDefaultOccupancyEstimateStep>
            {
                new CapDefaultOccupancyEstimateAdjuster(2113.0),
                new LabelDefaultOccupancyEstimateAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.94, ["north"] = 1.14, ["retail"] = 0.77 }),
                new TieredDefaultOccupancyEstimateAdjuster(
                    new List<double> { 171.0, 458.0, 2695.0, 7969.0 },
                    new List<double> { 0.85, 1.18, 0.94, 1.12 }),
                new RoundingDefaultOccupancyEstimateAdjuster(1.0),
            },
            new List<string>(),
            3.1,
            58451.7);
    }

    public double Compute(DefaultOccupancyEstimate item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultOccupancyEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
