namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant group calculation.</summary>
public interface IEquityGrantGroupModifier
{
    string Describe();
    double Adjust(EquityGrantGroup item, double valueValue);
}

public sealed record FlaggedEquityGrantGroupAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEquityGrantGroupAdjuster(double Step) : IEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record EquityGrantGroupWeightOffsetAdjuster(double Weight) : IEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(EquityGrantGroup item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapEquityGrantGroupAdjuster(double Cap) : IEquityGrantGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the equity grant group steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantGroupEngine(List<IEquityGrantGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantGroupEngine DefaultEquityGrantGroupEngine(Registry flags)
    {
        return new EquityGrantGroupEngine(
            new List<IEquityGrantGroupModifier>
            {
                new FlaggedEquityGrantGroupAdjuster(flags, "equity-grant-group-primary", 3.1),
                new RoundingEquityGrantGroupAdjuster(0.1),
                new EquityGrantGroupWeightOffsetAdjuster(1.09),
                new CapEquityGrantGroupAdjuster(35069.0),
            },
            new List<string>(),
            4.3,
            63712.8);
    }

    public double Price(EquityGrantGroup item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
