namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict policy entry calculation.</summary>
public interface IStrictPolicyEntryFactor
{
    string Describe();
    double Adjust(StrictPolicyEntry item, double valueValue);
}

public sealed record StrictPolicyEntryMinimumOffsetAdjuster(double Weight) : IStrictPolicyEntryFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(StrictPolicyEntry item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingStrictPolicyEntryAdjuster(double Step) : IStrictPolicyEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPolicyEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameStrictPolicyEntryAdjuster(Dictionary<string, double> Factors) : IStrictPolicyEntryFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictPolicyEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictPolicyEntryAdjuster(double Percent) : IStrictPolicyEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPolicyEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictPolicyEntryAdjuster(List<double> Bounds, List<double> Rates) : IStrictPolicyEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPolicyEntry item, double valueValue)
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

public sealed record FlaggedStrictPolicyEntryAdjuster(Registry Flags, string Flag, double Percent) : IStrictPolicyEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPolicyEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict policy entry steps in order and keeps a trace of each.</summary>
public sealed class StrictPolicyEntryEngine(List<IStrictPolicyEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPolicyEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPolicyEntryEngine DefaultStrictPolicyEntryEngine(Registry flags)
    {
        return new StrictPolicyEntryEngine(
            new List<IStrictPolicyEntryFactor>
            {
                new StrictPolicyEntryMinimumOffsetAdjuster(1.04),
                new RoundingStrictPolicyEntryAdjuster(0.5),
                new NameStrictPolicyEntryAdjuster(new Dictionary<string, double> { ["north"] = 1.27 }),
                new PercentStrictPolicyEntryAdjuster(13.2),
                new TieredStrictPolicyEntryAdjuster(
                    new List<double> { 253.0, 7690.0 },
                    new List<double> { 0.96, 0.94 }),
                new FlaggedStrictPolicyEntryAdjuster(flags, "strict-policy-entry-weighted", -9.9),
            },
            new List<string>(),
            3.0,
            60654.4);
    }

    public double Price(StrictPolicyEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictPolicyEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
