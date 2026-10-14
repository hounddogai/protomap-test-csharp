namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy position template calculation.</summary>
public interface ILegacyPositionTemplateFactor
{
    string Describe();
    double Adjust(LegacyPositionTemplate item, double valueValue);
}

public sealed record FlaggedLegacyPositionTemplateAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPositionTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPositionTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyPositionTemplateAdjuster(double Cap) : ILegacyPositionTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPositionTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyPositionTemplateAdjuster(double Minimum) : ILegacyPositionTemplateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPositionTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyPositionTemplateAdjuster(double Step) : ILegacyPositionTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPositionTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy position template steps in order and keeps a trace of each.</summary>
public sealed class LegacyPositionTemplateEngine(List<ILegacyPositionTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPositionTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPositionTemplateEngine DefaultLegacyPositionTemplateEngine(Registry flags)
    {
        return new LegacyPositionTemplateEngine(
            new List<ILegacyPositionTemplateFactor>
            {
                new FlaggedLegacyPositionTemplateAdjuster(flags, "legacy-position-template-fallback", -5.5),
                new CapLegacyPositionTemplateAdjuster(46601.0),
                new MinimumLegacyPositionTemplateAdjuster(28.7),
                new RoundingLegacyPositionTemplateAdjuster(1.0),
            },
            new List<string>(),
            0.2,
            34880.8);
    }

    public double Price(LegacyPositionTemplate item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPositionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
