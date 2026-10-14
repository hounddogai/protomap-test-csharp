namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual access badge budget calculation.</summary>
public interface IManualAccessBadgeBudgetStep
{
    string Describe();
    double Adjust(ManualAccessBadgeBudget item, double valueValue);
}

public sealed record FlaggedManualAccessBadgeBudgetAdjuster(Registry Flags, string Flag, double Percent) : IManualAccessBadgeBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAccessBadgeBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualAccessBadgeBudgetAdjuster(double Step) : IManualAccessBadgeBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAccessBadgeBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitManualAccessBadgeBudgetAdjuster(Dictionary<string, double> Factors) : IManualAccessBadgeBudgetStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ManualAccessBadgeBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual access badge budget steps in order and keeps a trace of each.</summary>
public sealed class ManualAccessBadgeBudgetEngine(List<IManualAccessBadgeBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAccessBadgeBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAccessBadgeBudgetEngine DefaultManualAccessBadgeBudgetEngine(Registry flags)
    {
        return new ManualAccessBadgeBudgetEngine(
            new List<IManualAccessBadgeBudgetStep>
            {
                new FlaggedManualAccessBadgeBudgetAdjuster(flags, "manual-access-badge-budget-primary", 4.7),
                new RoundingManualAccessBadgeBudgetAdjuster(0.05),
                new UnitManualAccessBadgeBudgetAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.3, ["retail"] = 0.74 }),
            },
            new List<string>(),
            4.0,
            75677.4);
    }

    public double Run(ManualAccessBadgeBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualAccessBadgeBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
