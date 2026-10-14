namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the warranty checkpoint calculation.</summary>
public interface IWarrantyCheckpointFactor
{
    string Describe();
    double Adjust(WarrantyCheckpoint item, double valueValue);
}

public sealed record TieredWarrantyCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyCheckpoint item, double valueValue)
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

public sealed record FlaggedWarrantyCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleWarrantyCheckpointAdjuster(Dictionary<string, double> Factors) : IWarrantyCheckpointFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WarrantyCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWarrantyCheckpointAdjuster(double Cap) : IWarrantyCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the warranty checkpoint steps in order and keeps a trace of each.</summary>
public sealed class WarrantyCheckpointEngine(List<IWarrantyCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyCheckpointEngine DefaultWarrantyCheckpointEngine(Registry flags)
    {
        return new WarrantyCheckpointEngine(
            new List<IWarrantyCheckpointFactor>
            {
                new TieredWarrantyCheckpointAdjuster(
                    new List<double> { 1031.0, 5980.0, 8255.0, 9800.0 },
                    new List<double> { 1.15, 1.09, 0.89, 1.09 }),
                new FlaggedWarrantyCheckpointAdjuster(flags, "warranty-checkpoint-capped", -1.6),
                new TitleWarrantyCheckpointAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.3, ["west"] = 1.26 }),
                new CapWarrantyCheckpointAdjuster(14284.0),
            },
            new List<string>(),
            5.0,
            96421.9);
    }

    public double Compute(WarrantyCheckpoint item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
