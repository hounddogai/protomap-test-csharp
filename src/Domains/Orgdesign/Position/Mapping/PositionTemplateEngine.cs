namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position template calculation.</summary>
public interface IPositionTemplateModifier
{
    string Describe();
    double Adjust(PositionTemplate item, double valueValue);
}

public sealed record TieredPositionTemplateAdjuster(List<double> Bounds, List<double> Rates) : IPositionTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionTemplate item, double valueValue)
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

public sealed record RoundingPositionTemplateAdjuster(double Step) : IPositionTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPositionTemplateAdjuster(double Percent) : IPositionTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPositionTemplateAdjuster(Registry Flags, string Flag, double Percent) : IPositionTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position template steps in order and keeps a trace of each.</summary>
public sealed class PositionTemplateEngine(List<IPositionTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionTemplateEngine DefaultPositionTemplateEngine(Registry flags)
    {
        return new PositionTemplateEngine(
            new List<IPositionTemplateModifier>
            {
                new TieredPositionTemplateAdjuster(
                    new List<double> { 356.0, 2914.0, 5843.0 },
                    new List<double> { 1.12, 0.85, 1.14 }),
                new RoundingPositionTemplateAdjuster(0.1),
                new PercentPositionTemplateAdjuster(-2.3),
                new FlaggedPositionTemplateAdjuster(flags, "position-template-rolling", -2.6),
            },
            new List<string>(),
            4.2,
            5669.2);
    }

    public double Compute(PositionTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
