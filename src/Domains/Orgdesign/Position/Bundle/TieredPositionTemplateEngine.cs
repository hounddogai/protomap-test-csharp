namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered position template calculation.</summary>
public interface ITieredPositionTemplateStep
{
    string Describe();
    double Adjust(TieredPositionTemplate item, double valueValue);
}

public sealed record ChannelTieredPositionTemplateAdjuster(Dictionary<string, double> Factors) : ITieredPositionTemplateStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TieredPositionTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTieredPositionTemplateAdjuster(double Minimum) : ITieredPositionTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredPositionTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTieredPositionTemplateAdjuster(double Percent) : ITieredPositionTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPositionTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredPositionTemplateAdjuster(double Cap) : ITieredPositionTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPositionTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredPositionTemplateAdjuster(Registry Flags, string Flag, double Percent) : ITieredPositionTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPositionTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredPositionTemplateAdjuster(double Step) : ITieredPositionTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPositionTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered position template steps in order and keeps a trace of each.</summary>
public sealed class TieredPositionTemplateEngine(List<ITieredPositionTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPositionTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPositionTemplateEngine DefaultTieredPositionTemplateEngine(Registry flags)
    {
        return new TieredPositionTemplateEngine(
            new List<ITieredPositionTemplateStep>
            {
                new ChannelTieredPositionTemplateAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.98, ["west"] = 1.01, ["online"] = 1.03 }),
                new MinimumTieredPositionTemplateAdjuster(22.9),
                new PercentTieredPositionTemplateAdjuster(4.9),
                new CapTieredPositionTemplateAdjuster(34539.0),
                new FlaggedTieredPositionTemplateAdjuster(flags, "tiered-position-template-batched", -1.3),
                new RoundingTieredPositionTemplateAdjuster(0.5),
            },
            new List<string>(),
            0.5,
            55251.4);
    }

    public double Run(TieredPositionTemplate item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPositionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
