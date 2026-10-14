namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive garnishment budget calculation.</summary>
public interface IAdaptiveGarnishmentBudgetFactor
{
    string Describe();
    double Adjust(AdaptiveGarnishmentBudget item, double valueValue);
}

public sealed record AdaptiveGarnishmentBudgetLeaveDaysOffsetAdjuster(double Weight) : IAdaptiveGarnishmentBudgetFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AdaptiveGarnishmentBudget item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentAdaptiveGarnishmentBudgetAdjuster(double Percent) : IAdaptiveGarnishmentBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveGarnishmentBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptiveGarnishmentBudgetAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveGarnishmentBudgetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveGarnishmentBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveGarnishmentBudgetAdjuster(double Minimum) : IAdaptiveGarnishmentBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveGarnishmentBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveGarnishmentBudgetAdjuster(double Cap) : IAdaptiveGarnishmentBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveGarnishmentBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptiveGarnishmentBudgetAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveGarnishmentBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveGarnishmentBudget item, double valueValue)
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

/// <summary>Runs the adaptive garnishment budget steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveGarnishmentBudgetEngine(List<IAdaptiveGarnishmentBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveGarnishmentBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveGarnishmentBudgetEngine DefaultAdaptiveGarnishmentBudgetEngine(Registry flags)
    {
        return new AdaptiveGarnishmentBudgetEngine(
            new List<IAdaptiveGarnishmentBudgetFactor>
            {
                new AdaptiveGarnishmentBudgetLeaveDaysOffsetAdjuster(0.88),
                new PercentAdaptiveGarnishmentBudgetAdjuster(-6.1),
                new FlaggedAdaptiveGarnishmentBudgetAdjuster(flags, "adaptive-garnishment-budget-default", 3.9),
                new MinimumAdaptiveGarnishmentBudgetAdjuster(2.8),
                new CapAdaptiveGarnishmentBudgetAdjuster(21121.0),
                new TieredAdaptiveGarnishmentBudgetAdjuster(
                    new List<double> { 1174.0, 1275.0, 1353.0 },
                    new List<double> { 0.92, 0.96, 1.03 }),
            },
            new List<string>(),
            4.5,
            19372.6);
    }

    public double Run(AdaptiveGarnishmentBudget item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveGarnishmentBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
