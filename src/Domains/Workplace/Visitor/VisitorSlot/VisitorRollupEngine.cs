namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor rollup calculation.</summary>
public interface IVisitorRollupModifier
{
    string Describe();
    double Adjust(VisitorRollup item, double valueValue);
}

public sealed record MinimumVisitorRollupAdjuster(double Minimum) : IVisitorRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingVisitorRollupAdjuster(double Step) : IVisitorRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedVisitorRollupAdjuster(Registry Flags, string Flag, double Percent) : IVisitorRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record VisitorRollupExpiresAtOffsetAdjuster(double Weight) : IVisitorRollupModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(VisitorRollup item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentVisitorRollupAdjuster(double Percent) : IVisitorRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor rollup steps in order and keeps a trace of each.</summary>
public sealed class VisitorRollupEngine(List<IVisitorRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorRollupEngine DefaultVisitorRollupEngine(Registry flags)
    {
        return new VisitorRollupEngine(
            new List<IVisitorRollupModifier>
            {
                new MinimumVisitorRollupAdjuster(27.0),
                new RoundingVisitorRollupAdjuster(0.5),
                new FlaggedVisitorRollupAdjuster(flags, "visitor-rollup-seasonal", -8.8),
                new VisitorRollupExpiresAtOffsetAdjuster(1.11),
                new PercentVisitorRollupAdjuster(16.1),
            },
            new List<string>(),
            2.5,
            64703.0);
    }

    public double Price(VisitorRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
