namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh budget calculation.</summary>
public interface IRefreshBudgetStep
{
    string Describe();
    double Adjust(RefreshBudget item, double valueValue);
}

public sealed record PercentRefreshBudgetAdjuster(double Percent) : IRefreshBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryRefreshBudgetAdjuster(Dictionary<string, double> Factors) : IRefreshBudgetStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RefreshBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRefreshBudgetAdjuster(double Step) : IRefreshBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRefreshBudgetAdjuster(double Minimum) : IRefreshBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRefreshBudgetAdjuster(double Cap) : IRefreshBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the refresh budget steps in order and keeps a trace of each.</summary>
public sealed class RefreshBudgetEngine(List<IRefreshBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshBudgetEngine DefaultRefreshBudgetEngine()
    {
        return new RefreshBudgetEngine(
            new List<IRefreshBudgetStep>
            {
                new PercentRefreshBudgetAdjuster(-21.2),
                new CountryRefreshBudgetAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.12, ["south"] = 1.17 }),
                new RoundingRefreshBudgetAdjuster(0.1),
                new MinimumRefreshBudgetAdjuster(38.3),
                new CapRefreshBudgetAdjuster(16211.0),
            },
            new List<string>(),
            4.1,
            20157.1);
    }

    public double Evaluate(RefreshBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<RefreshBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
