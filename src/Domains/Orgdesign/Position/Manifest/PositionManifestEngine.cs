namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position manifest calculation.</summary>
public interface IPositionManifestAdjuster
{
    string Describe();
    double Adjust(PositionManifest item, double valueValue);
}

public sealed record PositionManifestDurationMinutesOffsetAdjuster(double Weight) : IPositionManifestAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PositionManifest item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record TieredPositionManifestAdjuster(List<double> Bounds, List<double> Rates) : IPositionManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionManifest item, double valueValue)
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

public sealed record FlaggedPositionManifestAdjuster(Registry Flags, string Flag, double Percent) : IPositionManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPositionManifestAdjuster(double Minimum) : IPositionManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPositionManifestAdjuster(double Percent) : IPositionManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPositionManifestAdjuster(double Step) : IPositionManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the position manifest steps in order and keeps a trace of each.</summary>
public sealed class PositionManifestEngine(List<IPositionManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionManifestEngine DefaultPositionManifestEngine(Registry flags)
    {
        return new PositionManifestEngine(
            new List<IPositionManifestAdjuster>
            {
                new PositionManifestDurationMinutesOffsetAdjuster(0.11),
                new TieredPositionManifestAdjuster(
                    new List<double> { 2037.0, 8957.0 },
                    new List<double> { 0.97, 1.14 }),
                new FlaggedPositionManifestAdjuster(flags, "position-manifest-batched", 0.9),
                new MinimumPositionManifestAdjuster(25.8),
                new PercentPositionManifestAdjuster(-3.3),
                new RoundingPositionManifestAdjuster(5.0),
            },
            new List<string>(),
            2.9,
            86863.0);
    }

    public double Compute(PositionManifest item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
