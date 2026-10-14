namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility target calculation.</summary>
public interface IMobilityTargetStep
{
    string Describe();
    double Adjust(MobilityTarget item, double valueValue);
}

public sealed record TieredMobilityTargetAdjuster(List<double> Bounds, List<double> Rates) : IMobilityTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityTarget item, double valueValue)
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

public sealed record FlaggedMobilityTargetAdjuster(Registry Flags, string Flag, double Percent) : IMobilityTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMobilityTargetAdjuster(double Cap) : IMobilityTargetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the mobility target steps in order and keeps a trace of each.</summary>
public sealed class MobilityTargetEngine(List<IMobilityTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityTargetEngine DefaultMobilityTargetEngine(Registry flags)
    {
        return new MobilityTargetEngine(
            new List<IMobilityTargetStep>
            {
                new TieredMobilityTargetAdjuster(
                    new List<double> { 1286.0, 5781.0, 6118.0 },
                    new List<double> { 0.98, 0.85, 1.16 }),
                new FlaggedMobilityTargetAdjuster(flags, "mobility-target-strict", 3.5),
                new CapMobilityTargetAdjuster(46761.0),
            },
            new List<string>(),
            0.5,
            18157.8);
    }

    public double Run(MobilityTarget item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
