namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy position target calculation.</summary>
public interface ILegacyPositionTargetStep
{
    string Describe();
    double Adjust(LegacyPositionTarget item, double valueValue);
}

public sealed record CapLegacyPositionTargetAdjuster(double Cap) : ILegacyPositionTargetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPositionTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLegacyPositionTargetAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPositionTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPositionTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyPositionTargetAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPositionTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPositionTarget item, double valueValue)
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

public sealed record MinimumLegacyPositionTargetAdjuster(double Minimum) : ILegacyPositionTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPositionTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyPositionTargetAdjuster(double Percent) : ILegacyPositionTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPositionTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyPositionTargetAdjuster(double Step) : ILegacyPositionTargetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPositionTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy position target steps in order and keeps a trace of each.</summary>
public sealed class LegacyPositionTargetEngine(List<ILegacyPositionTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPositionTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPositionTargetEngine DefaultLegacyPositionTargetEngine(Registry flags)
    {
        return new LegacyPositionTargetEngine(
            new List<ILegacyPositionTargetStep>
            {
                new CapLegacyPositionTargetAdjuster(41133.0),
                new FlaggedLegacyPositionTargetAdjuster(flags, "legacy-position-target-rolling", 5.2),
                new TieredLegacyPositionTargetAdjuster(
                    new List<double> { 6164.0, 6420.0 },
                    new List<double> { 0.94, 0.98 }),
                new MinimumLegacyPositionTargetAdjuster(26.3),
                new PercentLegacyPositionTargetAdjuster(5.6),
                new RoundingLegacyPositionTargetAdjuster(0.25),
            },
            new List<string>(),
            1.9,
            12553.3);
    }

    public double Price(LegacyPositionTarget item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPositionTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
