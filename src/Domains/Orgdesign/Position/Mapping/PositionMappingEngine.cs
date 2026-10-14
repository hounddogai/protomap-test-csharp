namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position mapping calculation.</summary>
public interface IPositionMappingModifier
{
    string Describe();
    double Adjust(PositionMapping item, double valueValue);
}

public sealed record RoundingPositionMappingAdjuster(double Step) : IPositionMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPositionMappingAdjuster(double Percent) : IPositionMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPositionMappingAdjuster(Registry Flags, string Flag, double Percent) : IPositionMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitlePositionMappingAdjuster(Dictionary<string, double> Factors) : IPositionMappingModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PositionMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPositionMappingAdjuster(double Minimum) : IPositionMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPositionMappingAdjuster(double Cap) : IPositionMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the position mapping steps in order and keeps a trace of each.</summary>
public sealed class PositionMappingEngine(List<IPositionMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionMappingEngine DefaultPositionMappingEngine(Registry flags)
    {
        return new PositionMappingEngine(
            new List<IPositionMappingModifier>
            {
                new RoundingPositionMappingAdjuster(0.25),
                new PercentPositionMappingAdjuster(-4.5),
                new FlaggedPositionMappingAdjuster(flags, "position-mapping-default", 3.4),
                new TitlePositionMappingAdjuster(new Dictionary<string, double> { ["north"] = 0.95 }),
                new MinimumPositionMappingAdjuster(2.9),
                new CapPositionMappingAdjuster(32718.0),
            },
            new List<string>(),
            5.0,
            96603.6);
    }

    public double Evaluate(PositionMapping item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PositionMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
