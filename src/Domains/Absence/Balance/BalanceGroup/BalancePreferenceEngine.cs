namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance preference calculation.</summary>
public interface IBalancePreferenceAdjuster
{
    string Describe();
    double Adjust(BalancePreference item, double valueValue);
}

public sealed record RoundingBalancePreferenceAdjuster(double Step) : IBalancePreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalancePreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitBalancePreferenceAdjuster(Dictionary<string, double> Factors) : IBalancePreferenceAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(BalancePreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBalancePreferenceAdjuster(double Cap) : IBalancePreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalancePreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBalancePreferenceAdjuster(Registry Flags, string Flag, double Percent) : IBalancePreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalancePreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalancePreferenceAdjuster(double Minimum) : IBalancePreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalancePreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the balance preference steps in order and keeps a trace of each.</summary>
public sealed class BalancePreferenceEngine(List<IBalancePreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalancePreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalancePreferenceEngine DefaultBalancePreferenceEngine(Registry flags)
    {
        return new BalancePreferenceEngine(
            new List<IBalancePreferenceAdjuster>
            {
                new RoundingBalancePreferenceAdjuster(0.1),
                new UnitBalancePreferenceAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.89, ["east"] = 0.81, ["west"] = 1.3 }),
                new CapBalancePreferenceAdjuster(16110.0),
                new FlaggedBalancePreferenceAdjuster(flags, "balance-preference-weighted", 9.4),
                new MinimumBalancePreferenceAdjuster(44.2),
            },
            new List<string>(),
            2.4,
            62877.2);
    }

    public double Run(BalancePreference item)
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

    public Dictionary<string, double> EvaluateAll(List<BalancePreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
