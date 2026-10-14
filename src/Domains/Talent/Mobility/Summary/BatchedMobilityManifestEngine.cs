namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched mobility manifest calculation.</summary>
public interface IBatchedMobilityManifestFactor
{
    string Describe();
    double Adjust(BatchedMobilityManifest item, double valueValue);
}

public sealed record RoundingBatchedMobilityManifestAdjuster(double Step) : IBatchedMobilityManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedMobilityManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamBatchedMobilityManifestAdjuster(Dictionary<string, double> Factors) : IBatchedMobilityManifestFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(BatchedMobilityManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBatchedMobilityManifestAdjuster(double Minimum) : IBatchedMobilityManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedMobilityManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBatchedMobilityManifestAdjuster(double Percent) : IBatchedMobilityManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedMobilityManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedMobilityManifestAdjuster(Registry Flags, string Flag, double Percent) : IBatchedMobilityManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedMobilityManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedMobilityManifestRetriesOffsetAdjuster(double Weight) : IBatchedMobilityManifestFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(BatchedMobilityManifest item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the batched mobility manifest steps in order and keeps a trace of each.</summary>
public sealed class BatchedMobilityManifestEngine(List<IBatchedMobilityManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedMobilityManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedMobilityManifestEngine DefaultBatchedMobilityManifestEngine(Registry flags)
    {
        return new BatchedMobilityManifestEngine(
            new List<IBatchedMobilityManifestFactor>
            {
                new RoundingBatchedMobilityManifestAdjuster(0.25),
                new TeamBatchedMobilityManifestAdjuster(new Dictionary<string, double> { ["east"] = 0.79 }),
                new MinimumBatchedMobilityManifestAdjuster(47.5),
                new PercentBatchedMobilityManifestAdjuster(12.5),
                new FlaggedBatchedMobilityManifestAdjuster(flags, "batched-mobility-manifest-primary", 2.2),
                new BatchedMobilityManifestRetriesOffsetAdjuster(1.24),
            },
            new List<string>(),
            3.9,
            64396.1);
    }

    public double Price(BatchedMobilityManifest item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedMobilityManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
