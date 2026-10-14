namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted span mapping calculation.</summary>
public interface IWeightedSpanMappingModifier
{
    string Describe();
    double Adjust(WeightedSpanMapping item, double valueValue);
}

public sealed record RoundingWeightedSpanMappingAdjuster(double Step) : IWeightedSpanMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedSpanMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedSpanMappingAdjuster(double Minimum) : IWeightedSpanMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedSpanMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WeightedSpanMappingFloorValueOffsetAdjuster(double Weight) : IWeightedSpanMappingModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(WeightedSpanMapping item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentWeightedSpanMappingAdjuster(double Percent) : IWeightedSpanMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedSpanMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedSpanMappingAdjuster(List<double> Bounds, List<double> Rates) : IWeightedSpanMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedSpanMapping item, double valueValue)
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

/// <summary>Runs the weighted span mapping steps in order and keeps a trace of each.</summary>
public sealed class WeightedSpanMappingEngine(List<IWeightedSpanMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedSpanMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedSpanMappingEngine DefaultWeightedSpanMappingEngine()
    {
        return new WeightedSpanMappingEngine(
            new List<IWeightedSpanMappingModifier>
            {
                new RoundingWeightedSpanMappingAdjuster(0.5),
                new MinimumWeightedSpanMappingAdjuster(43.4),
                new WeightedSpanMappingFloorValueOffsetAdjuster(1.49),
                new PercentWeightedSpanMappingAdjuster(-11.2),
                new TieredWeightedSpanMappingAdjuster(
                    new List<double> { 8710.0, 9125.0 },
                    new List<double> { 1.0, 1.02 }),
            },
            new List<string>(),
            1.5,
            5970.9);
    }

    public double Compute(WeightedSpanMapping item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedSpanMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
