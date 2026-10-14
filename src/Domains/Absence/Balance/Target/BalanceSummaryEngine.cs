namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance summary calculation.</summary>
public interface IBalanceSummaryStep
{
    string Describe();
    double Adjust(BalanceSummary item, double valueValue);
}

public sealed record RoundingBalanceSummaryAdjuster(double Step) : IBalanceSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBalanceSummaryAdjuster(Registry Flags, string Flag, double Percent) : IBalanceSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceSummaryAdjuster(double Minimum) : IBalanceSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameBalanceSummaryAdjuster(Dictionary<string, double> Factors) : IBalanceSummaryStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BalanceSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance summary steps in order and keeps a trace of each.</summary>
public sealed class BalanceSummaryEngine(List<IBalanceSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceSummaryEngine DefaultBalanceSummaryEngine(Registry flags)
    {
        return new BalanceSummaryEngine(
            new List<IBalanceSummaryStep>
            {
                new RoundingBalanceSummaryAdjuster(0.5),
                new FlaggedBalanceSummaryAdjuster(flags, "balance-summary-seasonal", 4.4),
                new MinimumBalanceSummaryAdjuster(7.9),
                new NameBalanceSummaryAdjuster(new Dictionary<string, double> { ["retail"] = 1.16 }),
            },
            new List<string>(),
            4.6,
            39651.6);
    }

    public double Run(BalanceSummary item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
