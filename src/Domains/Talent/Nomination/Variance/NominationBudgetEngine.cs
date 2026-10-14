namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination budget calculation.</summary>
public interface INominationBudgetStep
{
    string Describe();
    double Adjust(NominationBudget item, double valueValue);
}

public sealed record TitleNominationBudgetAdjuster(Dictionary<string, double> Factors) : INominationBudgetStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(NominationBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumNominationBudgetAdjuster(double Minimum) : INominationBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapNominationBudgetAdjuster(double Cap) : INominationBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedNominationBudgetAdjuster(Registry Flags, string Flag, double Percent) : INominationBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the nomination budget steps in order and keeps a trace of each.</summary>
public sealed class NominationBudgetEngine(List<INominationBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationBudgetEngine DefaultNominationBudgetEngine(Registry flags)
    {
        return new NominationBudgetEngine(
            new List<INominationBudgetStep>
            {
                new TitleNominationBudgetAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.88, ["north"] = 1.03, ["retail"] = 0.75 }),
                new MinimumNominationBudgetAdjuster(26.5),
                new CapNominationBudgetAdjuster(45869.0),
                new FlaggedNominationBudgetAdjuster(flags, "nomination-budget-tiered", 0.5),
            },
            new List<string>(),
            1.3,
            44869.5);
    }

    public double Price(NominationBudget item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<NominationBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
