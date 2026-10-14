namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient holiday budget calculation.</summary>
public interface ILenientHolidayBudgetModifier
{
    string Describe();
    double Adjust(LenientHolidayBudget item, double valueValue);
}

public sealed record FlaggedLenientHolidayBudgetAdjuster(Registry Flags, string Flag, double Percent) : ILenientHolidayBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientHolidayBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientHolidayBudgetAdjuster(double Percent) : ILenientHolidayBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientHolidayBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientHolidayBudgetAdjuster(double Minimum) : ILenientHolidayBudgetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientHolidayBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient holiday budget steps in order and keeps a trace of each.</summary>
public sealed class LenientHolidayBudgetEngine(List<ILenientHolidayBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientHolidayBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientHolidayBudgetEngine DefaultLenientHolidayBudgetEngine(Registry flags)
    {
        return new LenientHolidayBudgetEngine(
            new List<ILenientHolidayBudgetModifier>
            {
                new FlaggedLenientHolidayBudgetAdjuster(flags, "lenient-holiday-budget-rolling", 4.8),
                new PercentLenientHolidayBudgetAdjuster(9.0),
                new MinimumLenientHolidayBudgetAdjuster(27.8),
            },
            new List<string>(),
            3.4,
            11663.9);
    }

    public double Evaluate(LenientHolidayBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientHolidayBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
