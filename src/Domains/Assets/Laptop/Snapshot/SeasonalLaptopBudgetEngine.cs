namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal laptop budget calculation.</summary>
public interface ISeasonalLaptopBudgetFactor
{
    string Describe();
    double Adjust(SeasonalLaptopBudget item, double valueValue);
}

public sealed record RoundingSeasonalLaptopBudgetAdjuster(double Step) : ISeasonalLaptopBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalLaptopBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalLaptopBudgetAdjuster(double Minimum) : ISeasonalLaptopBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalLaptopBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SeasonalLaptopBudgetThresholdOffsetAdjuster(double Weight) : ISeasonalLaptopBudgetFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(SeasonalLaptopBudget item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedSeasonalLaptopBudgetAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalLaptopBudgetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalLaptopBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal laptop budget steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLaptopBudgetEngine(List<ISeasonalLaptopBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLaptopBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLaptopBudgetEngine DefaultSeasonalLaptopBudgetEngine(Registry flags)
    {
        return new SeasonalLaptopBudgetEngine(
            new List<ISeasonalLaptopBudgetFactor>
            {
                new RoundingSeasonalLaptopBudgetAdjuster(0.5),
                new MinimumSeasonalLaptopBudgetAdjuster(34.3),
                new SeasonalLaptopBudgetThresholdOffsetAdjuster(0.28),
                new FlaggedSeasonalLaptopBudgetAdjuster(flags, "seasonal-laptop-budget-regional", -3.0),
            },
            new List<string>(),
            2.8,
            20841.3);
    }

    public double Compute(SeasonalLaptopBudget item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLaptopBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
