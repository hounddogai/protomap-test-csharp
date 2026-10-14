namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default headcount entry calculation.</summary>
public interface IDefaultHeadcountEntryStep
{
    string Describe();
    double Adjust(DefaultHeadcountEntry item, double valueValue);
}

public sealed record CapDefaultHeadcountEntryAdjuster(double Cap) : IDefaultHeadcountEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultHeadcountEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelDefaultHeadcountEntryAdjuster(Dictionary<string, double> Factors) : IDefaultHeadcountEntryStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultHeadcountEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultHeadcountEntryAdjuster(List<double> Bounds, List<double> Rates) : IDefaultHeadcountEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultHeadcountEntry item, double valueValue)
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

public sealed record FlaggedDefaultHeadcountEntryAdjuster(Registry Flags, string Flag, double Percent) : IDefaultHeadcountEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultHeadcountEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultHeadcountEntryAdjuster(double Minimum) : IDefaultHeadcountEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultHeadcountEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingDefaultHeadcountEntryAdjuster(double Step) : IDefaultHeadcountEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultHeadcountEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default headcount entry steps in order and keeps a trace of each.</summary>
public sealed class DefaultHeadcountEntryEngineExtra(List<IDefaultHeadcountEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultHeadcountEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public double Price(DefaultHeadcountEntry item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultHeadcountEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}

/// <summary>Functions of default headcount entry engine.</summary>
public static class DefaultHeadcountEntryEngine
{
    public static DefaultHeadcountEntryEngineExtra DefaultDefaultHeadcountEntryEngine(Registry flags)
    {
        return new DefaultHeadcountEntryEngineExtra(
            new List<IDefaultHeadcountEntryStep>
            {
                new CapDefaultHeadcountEntryAdjuster(15574.0),
                new LabelDefaultHeadcountEntryAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.28, ["retail"] = 1.05, ["north"] = 1.04 }),
                new TieredDefaultHeadcountEntryAdjuster(
                    new List<double> { 154.0, 512.0, 1019.0, 7375.0 },
                    new List<double> { 0.9, 0.91, 0.88, 1.11 }),
                new FlaggedDefaultHeadcountEntryAdjuster(flags, "default-headcount-entry-strict", -5.2),
                new MinimumDefaultHeadcountEntryAdjuster(29.3),
                new RoundingDefaultHeadcountEntryAdjuster(5.0),
            },
            new List<string>(),
            2.6,
            47664.1);
    }
}
