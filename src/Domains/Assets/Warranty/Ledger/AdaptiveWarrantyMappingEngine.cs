namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive warranty mapping calculation.</summary>
public interface IAdaptiveWarrantyMappingFactor
{
    string Describe();
    double Adjust(AdaptiveWarrantyMapping item, double valueValue);
}

public sealed record RoundingAdaptiveWarrantyMappingAdjuster(double Step) : IAdaptiveWarrantyMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveWarrantyMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelAdaptiveWarrantyMappingAdjuster(Dictionary<string, double> Factors) : IAdaptiveWarrantyMappingFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdaptiveWarrantyMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AdaptiveWarrantyMappingExpiresAtOffsetAdjuster(double Weight) : IAdaptiveWarrantyMappingFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(AdaptiveWarrantyMapping item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record CapAdaptiveWarrantyMappingAdjuster(double Cap) : IAdaptiveWarrantyMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveWarrantyMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAdaptiveWarrantyMappingAdjuster(double Minimum) : IAdaptiveWarrantyMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveWarrantyMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive warranty mapping steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveWarrantyMappingEngine(List<IAdaptiveWarrantyMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveWarrantyMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveWarrantyMappingEngine DefaultAdaptiveWarrantyMappingEngine()
    {
        return new AdaptiveWarrantyMappingEngine(
            new List<IAdaptiveWarrantyMappingFactor>
            {
                new RoundingAdaptiveWarrantyMappingAdjuster(1.0),
                new LabelAdaptiveWarrantyMappingAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.33, ["retail"] = 0.74 }),
                new AdaptiveWarrantyMappingExpiresAtOffsetAdjuster(0.58),
                new CapAdaptiveWarrantyMappingAdjuster(42053.0),
                new MinimumAdaptiveWarrantyMappingAdjuster(5.8),
            },
            new List<string>(),
            4.2,
            41767.5);
    }

    public double Run(AdaptiveWarrantyMapping item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveWarrantyMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
