namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted warranty mapping calculation.</summary>
public interface IWeightedWarrantyMappingStep
{
    string Describe();
    double Adjust(WeightedWarrantyMapping item, double valueValue);
}

public sealed record CapWeightedWarrantyMappingAdjuster(double Cap) : IWeightedWarrantyMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedWarrantyMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record WeightedWarrantyMappingWindowDaysOffsetAdjuster(double Weight) : IWeightedWarrantyMappingStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(WeightedWarrantyMapping item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record RoundingWeightedWarrantyMappingAdjuster(double Step) : IWeightedWarrantyMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedWarrantyMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedWarrantyMappingAdjuster(double Minimum) : IWeightedWarrantyMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedWarrantyMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredWeightedWarrantyMappingAdjuster(List<double> Bounds, List<double> Rates) : IWeightedWarrantyMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedWarrantyMapping item, double valueValue)
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

public sealed record LabelWeightedWarrantyMappingAdjuster(Dictionary<string, double> Factors) : IWeightedWarrantyMappingStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedWarrantyMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted warranty mapping steps in order and keeps a trace of each.</summary>
public sealed class WeightedWarrantyMappingEngine(List<IWeightedWarrantyMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedWarrantyMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedWarrantyMappingEngine DefaultWeightedWarrantyMappingEngine()
    {
        return new WeightedWarrantyMappingEngine(
            new List<IWeightedWarrantyMappingStep>
            {
                new CapWeightedWarrantyMappingAdjuster(3944.0),
                new WeightedWarrantyMappingWindowDaysOffsetAdjuster(0.95),
                new RoundingWeightedWarrantyMappingAdjuster(1.0),
                new MinimumWeightedWarrantyMappingAdjuster(15.1),
                new TieredWeightedWarrantyMappingAdjuster(
                    new List<double> { 625.0, 1434.0, 3293.0 },
                    new List<double> { 1.18, 1.04, 1.04 }),
                new LabelWeightedWarrantyMappingAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.8, ["retail"] = 0.91, ["central"] = 1.22 }),
            },
            new List<string>(),
            4.2,
            5718.2);
    }

    public double Compute(WeightedWarrantyMapping item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedWarrantyMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
