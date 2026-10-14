namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge mapping calculation.</summary>
public interface IAccessBadgeMappingAdjuster
{
    string Describe();
    double Adjust(AccessBadgeMapping item, double valueValue);
}

public sealed record MinimumAccessBadgeMappingAdjuster(double Minimum) : IAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAccessBadgeMappingAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccessBadgeMappingCeilingOffsetAdjuster(double Weight) : IAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(AccessBadgeMapping item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record TieredAccessBadgeMappingAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeMapping item, double valueValue)
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

public sealed record PercentAccessBadgeMappingAdjuster(double Percent) : IAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeMappingAdjuster(double Step) : IAccessBadgeMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the access badge mapping steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeMappingEngine(List<IAccessBadgeMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeMappingEngine DefaultAccessBadgeMappingEngine(Registry flags)
    {
        return new AccessBadgeMappingEngine(
            new List<IAccessBadgeMappingAdjuster>
            {
                new MinimumAccessBadgeMappingAdjuster(30.2),
                new FlaggedAccessBadgeMappingAdjuster(flags, "access-badge-mapping-global", -3.0),
                new AccessBadgeMappingCeilingOffsetAdjuster(0.44),
                new TieredAccessBadgeMappingAdjuster(
                    new List<double> { 4166.0, 5696.0 },
                    new List<double> { 0.9, 0.91 }),
                new PercentAccessBadgeMappingAdjuster(-3.1),
                new RoundingAccessBadgeMappingAdjuster(5.0),
            },
            new List<string>(),
            4.7,
            73400.2);
    }

    public double Price(AccessBadgeMapping item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
