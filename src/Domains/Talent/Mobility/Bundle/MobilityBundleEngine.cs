namespace Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>One step of the mobility bundle calculation.</summary>
public interface IMobilityBundleAdjuster
{
    string Describe();
    double Adjust(MobilityBundle item, double valueValue);
}

public sealed record CapMobilityBundleAdjuster(double Cap) : IMobilityBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MobilityBundleTenureMonthsOffsetAdjuster(double Weight) : IMobilityBundleAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(MobilityBundle item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record MinimumMobilityBundleAdjuster(double Minimum) : IMobilityBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredMobilityBundleAdjuster(List<double> Bounds, List<double> Rates) : IMobilityBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityBundle item, double valueValue)
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

/// <summary>Runs the mobility bundle steps in order and keeps a trace of each.</summary>
public sealed class MobilityBundleEngine(List<IMobilityBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityBundleEngine DefaultMobilityBundleEngine()
    {
        return new MobilityBundleEngine(
            new List<IMobilityBundleAdjuster>
            {
                new CapMobilityBundleAdjuster(29306.0),
                new MobilityBundleTenureMonthsOffsetAdjuster(1.94),
                new MinimumMobilityBundleAdjuster(49.5),
                new TieredMobilityBundleAdjuster(
                    new List<double> { 3288.0, 6054.0, 9309.0, 9422.0 },
                    new List<double> { 1.02, 1.13, 1.01, 0.89 }),
            },
            new List<string>(),
            0.6,
            91507.8);
    }

    public double Run(MobilityBundle item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
