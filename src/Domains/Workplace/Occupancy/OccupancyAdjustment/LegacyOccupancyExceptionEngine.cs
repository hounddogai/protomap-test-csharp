namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy occupancy exception calculation.</summary>
public interface ILegacyOccupancyExceptionFactor
{
    string Describe();
    double Adjust(LegacyOccupancyException item, double valueValue);
}

public sealed record PercentLegacyOccupancyExceptionAdjuster(double Percent) : ILegacyOccupancyExceptionFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyOccupancyException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyOccupancyExceptionAdjuster(double Cap) : ILegacyOccupancyExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyOccupancyException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleLegacyOccupancyExceptionAdjuster(Dictionary<string, double> Factors) : ILegacyOccupancyExceptionFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyOccupancyException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyOccupancyExceptionAdjuster(double Minimum) : ILegacyOccupancyExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyOccupancyException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyOccupancyExceptionAdjuster(double Step) : ILegacyOccupancyExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyOccupancyException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyOccupancyExceptionAdjuster(List<double> Bounds, List<double> Rates) : ILegacyOccupancyExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyOccupancyException item, double valueValue)
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

/// <summary>Runs the legacy occupancy exception steps in order and keeps a trace of each.</summary>
public sealed class LegacyOccupancyExceptionEngine(List<ILegacyOccupancyExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyOccupancyExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyOccupancyExceptionEngine DefaultLegacyOccupancyExceptionEngine()
    {
        return new LegacyOccupancyExceptionEngine(
            new List<ILegacyOccupancyExceptionFactor>
            {
                new PercentLegacyOccupancyExceptionAdjuster(21.9),
                new CapLegacyOccupancyExceptionAdjuster(13599.0),
                new TitleLegacyOccupancyExceptionAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.95, ["central"] = 1.15, ["east"] = 0.85 }),
                new MinimumLegacyOccupancyExceptionAdjuster(22.2),
                new RoundingLegacyOccupancyExceptionAdjuster(1.0),
                new TieredLegacyOccupancyExceptionAdjuster(
                    new List<double> { 2243.0, 2329.0, 9572.0 },
                    new List<double> { 0.98, 0.98, 1.02 }),
            },
            new List<string>(),
            0.2,
            72844.7);
    }

    public double Price(LegacyOccupancyException item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyOccupancyException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
