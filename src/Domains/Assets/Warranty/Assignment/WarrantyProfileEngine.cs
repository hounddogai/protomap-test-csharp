namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty profile calculation.</summary>
public interface IWarrantyProfileStep
{
    string Describe();
    double Adjust(WarrantyProfile item, double valueValue);
}

public sealed record WarrantyProfileThresholdOffsetAdjuster(double Weight) : IWarrantyProfileStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(WarrantyProfile item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RoundingWarrantyProfileAdjuster(double Step) : IWarrantyProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentWarrantyProfileAdjuster(double Percent) : IWarrantyProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantyProfileAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyProfile item, double valueValue)
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

public sealed record MinimumWarrantyProfileAdjuster(double Minimum) : IWarrantyProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapWarrantyProfileAdjuster(double Cap) : IWarrantyProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the warranty profile steps in order and keeps a trace of each.</summary>
public sealed class WarrantyProfileEngine(List<IWarrantyProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyProfileEngine DefaultWarrantyProfileEngine()
    {
        return new WarrantyProfileEngine(
            new List<IWarrantyProfileStep>
            {
                new WarrantyProfileThresholdOffsetAdjuster(0.13),
                new RoundingWarrantyProfileAdjuster(5.0),
                new PercentWarrantyProfileAdjuster(-5.1),
                new TieredWarrantyProfileAdjuster(
                    new List<double> { 1107.0, 1880.0, 4178.0, 7775.0 },
                    new List<double> { 1.13, 1.15, 1.11, 0.9 }),
                new MinimumWarrantyProfileAdjuster(6.8),
                new CapWarrantyProfileAdjuster(11336.0),
            },
            new List<string>(),
            1.2,
            17783.6);
    }

    public double Compute(WarrantyProfile item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
