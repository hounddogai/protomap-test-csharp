namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor override calculation.</summary>
public interface IVisitorOverrideFactor
{
    string Describe();
    double Adjust(VisitorOverride item, double valueValue);
}

public sealed record FlaggedVisitorOverrideAdjuster(Registry Flags, string Flag, double Percent) : IVisitorOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVisitorOverrideAdjuster(List<double> Bounds, List<double> Rates) : IVisitorOverrideFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorOverride item, double valueValue)
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

public sealed record NameVisitorOverrideAdjuster(Dictionary<string, double> Factors) : IVisitorOverrideFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(VisitorOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumVisitorOverrideAdjuster(double Minimum) : IVisitorOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapVisitorOverrideAdjuster(double Cap) : IVisitorOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVisitorOverrideAdjuster(double Step) : IVisitorOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the visitor override steps in order and keeps a trace of each.</summary>
public sealed class VisitorOverrideEngine(List<IVisitorOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorOverrideEngine DefaultVisitorOverrideEngine(Registry flags)
    {
        return new VisitorOverrideEngine(
            new List<IVisitorOverrideFactor>
            {
                new FlaggedVisitorOverrideAdjuster(flags, "visitor-override-adaptive", -2.4),
                new TieredVisitorOverrideAdjuster(
                    new List<double> { 1085.0, 2926.0, 3212.0, 9713.0 },
                    new List<double> { 0.87, 1.0, 1.0, 1.04 }),
                new NameVisitorOverrideAdjuster(new Dictionary<string, double> { ["north"] = 1.38 }),
                new MinimumVisitorOverrideAdjuster(10.4),
                new CapVisitorOverrideAdjuster(1814.0),
                new RoundingVisitorOverrideAdjuster(1.0),
            },
            new List<string>(),
            0.1,
            83758.0);
    }

    public double Evaluate(VisitorOverride item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
