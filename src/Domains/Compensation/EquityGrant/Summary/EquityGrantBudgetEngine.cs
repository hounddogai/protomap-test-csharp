namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant budget calculation.</summary>
public interface IEquityGrantBudgetAdjuster
{
    string Describe();
    double Adjust(EquityGrantBudget item, double valueValue);
}

public sealed record FlaggedEquityGrantBudgetAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEquityGrantBudgetAdjuster(double Minimum) : IEquityGrantBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingEquityGrantBudgetAdjuster(double Step) : IEquityGrantBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the equity grant budget steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantBudgetEngine(List<IEquityGrantBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantBudgetEngine DefaultEquityGrantBudgetEngine(Registry flags)
    {
        return new EquityGrantBudgetEngine(
            new List<IEquityGrantBudgetAdjuster>
            {
                new FlaggedEquityGrantBudgetAdjuster(flags, "equity-grant-budget-default", 1.8),
                new MinimumEquityGrantBudgetAdjuster(0.5),
                new RoundingEquityGrantBudgetAdjuster(1.0),
            },
            new List<string>(),
            0.4,
            78017.6);
    }

    public double Run(EquityGrantBudget item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
