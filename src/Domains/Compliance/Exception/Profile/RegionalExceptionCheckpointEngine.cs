namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional exception checkpoint calculation.</summary>
public interface IRegionalExceptionCheckpointModifier
{
    string Describe();
    double Adjust(RegionalExceptionCheckpoint item, double valueValue);
}

public sealed record PercentRegionalExceptionCheckpointAdjuster(double Percent) : IRegionalExceptionCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalExceptionCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalExceptionCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IRegionalExceptionCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalExceptionCheckpoint item, double valueValue)
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

public sealed record FlaggedRegionalExceptionCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IRegionalExceptionCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalExceptionCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalExceptionCheckpointAdjuster(double Minimum) : IRegionalExceptionCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalExceptionCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional exception checkpoint steps in order and keeps a trace of each.</summary>
public sealed class RegionalExceptionCheckpointEngine(List<IRegionalExceptionCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalExceptionCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalExceptionCheckpointEngine DefaultRegionalExceptionCheckpointEngine(Registry flags)
    {
        return new RegionalExceptionCheckpointEngine(
            new List<IRegionalExceptionCheckpointModifier>
            {
                new PercentRegionalExceptionCheckpointAdjuster(-5.5),
                new TieredRegionalExceptionCheckpointAdjuster(
                    new List<double> { 5042.0, 5927.0, 6176.0, 9250.0 },
                    new List<double> { 1.02, 0.96, 1.08, 0.88 }),
                new FlaggedRegionalExceptionCheckpointAdjuster(flags, "regional-exception-checkpoint-batched", -3.3),
                new MinimumRegionalExceptionCheckpointAdjuster(19.1),
            },
            new List<string>(),
            4.7,
            38824.0);
    }

    public double Compute(RegionalExceptionCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalExceptionCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
