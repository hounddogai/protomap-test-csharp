namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility manifest calculation.</summary>
public interface IMobilityManifestAdjuster
{
    string Describe();
    double Adjust(MobilityManifest item, double valueValue);
}

public sealed record TieredMobilityManifestAdjuster(List<double> Bounds, List<double> Rates) : IMobilityManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityManifest item, double valueValue)
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

public sealed record NameMobilityManifestAdjuster(Dictionary<string, double> Factors) : IMobilityManifestAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(MobilityManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapMobilityManifestAdjuster(double Cap) : IMobilityManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumMobilityManifestAdjuster(double Minimum) : IMobilityManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record MobilityManifestDurationMinutesOffsetAdjuster(double Weight) : IMobilityManifestAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(MobilityManifest item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the mobility manifest steps in order and keeps a trace of each.</summary>
public sealed class MobilityManifestEngine(List<IMobilityManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityManifestEngine DefaultMobilityManifestEngine()
    {
        return new MobilityManifestEngine(
            new List<IMobilityManifestAdjuster>
            {
                new TieredMobilityManifestAdjuster(
                    new List<double> { 621.0, 3323.0, 4050.0 },
                    new List<double> { 1.06, 0.89, 0.96 }),
                new NameMobilityManifestAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.35, ["east"] = 0.74, ["north"] = 1.04 }),
                new CapMobilityManifestAdjuster(8098.0),
                new MinimumMobilityManifestAdjuster(29.4),
                new MobilityManifestDurationMinutesOffsetAdjuster(0.52),
            },
            new List<string>(),
            1.4,
            23132.5);
    }

    public double Compute(MobilityManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<MobilityManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
