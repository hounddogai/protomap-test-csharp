namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility rollup calculation.</summary>
public interface IMobilityRollupFactor
{
    string Describe();
    double Adjust(MobilityRollup item, double valueValue);
}

public sealed record SourceMobilityRollupAdjuster(Dictionary<string, double> Factors) : IMobilityRollupFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(MobilityRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MobilityRollupMaximumOffsetAdjuster(double Weight) : IMobilityRollupFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(MobilityRollup item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record CapMobilityRollupAdjuster(double Cap) : IMobilityRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredMobilityRollupAdjuster(List<double> Bounds, List<double> Rates) : IMobilityRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityRollup item, double valueValue)
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

public sealed record PercentMobilityRollupAdjuster(double Percent) : IMobilityRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the mobility rollup steps in order and keeps a trace of each.</summary>
public sealed class MobilityRollupEngine(List<IMobilityRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityRollupEngine DefaultMobilityRollupEngine()
    {
        return new MobilityRollupEngine(
            new List<IMobilityRollupFactor>
            {
                new SourceMobilityRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.89, ["south"] = 1.29, ["north"] = 0.89 }),
                new MobilityRollupMaximumOffsetAdjuster(1.0),
                new CapMobilityRollupAdjuster(32509.0),
                new TieredMobilityRollupAdjuster(new List<double> { 1969.0, 2045.0 }, new List<double> { 1.11, 1.1 }),
                new PercentMobilityRollupAdjuster(12.5),
            },
            new List<string>(),
            0.1,
            85146.0);
    }

    public double Compute(MobilityRollup item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
