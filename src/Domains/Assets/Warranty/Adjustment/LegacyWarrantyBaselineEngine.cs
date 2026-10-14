namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy warranty baseline calculation.</summary>
public interface ILegacyWarrantyBaselineAdjuster
{
    string Describe();
    double Adjust(LegacyWarrantyBaseline item, double valueValue);
}

public sealed record RegionLegacyWarrantyBaselineAdjuster(Dictionary<string, double> Factors) : ILegacyWarrantyBaselineAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LegacyWarrantyBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLegacyWarrantyBaselineAdjuster(double Cap) : ILegacyWarrantyBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyWarrantyBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyWarrantyBaselineAdjuster(List<double> Bounds, List<double> Rates) : ILegacyWarrantyBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyWarrantyBaseline item, double valueValue)
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

public sealed record LegacyWarrantyBaselineCapacityOffsetAdjuster(double Weight) : ILegacyWarrantyBaselineAdjuster
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(LegacyWarrantyBaseline item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingLegacyWarrantyBaselineAdjuster(double Step) : ILegacyWarrantyBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyWarrantyBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLegacyWarrantyBaselineAdjuster(double Minimum) : ILegacyWarrantyBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyWarrantyBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy warranty baseline steps in order and keeps a trace of each.</summary>
public sealed class LegacyWarrantyBaselineEngine(List<ILegacyWarrantyBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyWarrantyBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyWarrantyBaselineEngine DefaultLegacyWarrantyBaselineEngine()
    {
        return new LegacyWarrantyBaselineEngine(
            new List<ILegacyWarrantyBaselineAdjuster>
            {
                new RegionLegacyWarrantyBaselineAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.01, ["north"] = 0.79 }),
                new CapLegacyWarrantyBaselineAdjuster(30207.0),
                new TieredLegacyWarrantyBaselineAdjuster(
                    new List<double> { 2379.0, 3249.0 },
                    new List<double> { 1.07, 1.15 }),
                new LegacyWarrantyBaselineCapacityOffsetAdjuster(1.39),
                new RoundingLegacyWarrantyBaselineAdjuster(0.1),
                new MinimumLegacyWarrantyBaselineAdjuster(29.1),
            },
            new List<string>(),
            3.3,
            21866.5);
    }

    public double Evaluate(LegacyWarrantyBaseline item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyWarrantyBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
