namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient garnishment budget calculation.</summary>
public interface ILenientGarnishmentBudgetAdjuster
{
    string Describe();
    double Adjust(LenientGarnishmentBudget item, double valueValue);
}

public sealed record FlaggedLenientGarnishmentBudgetAdjuster(Registry Flags, string Flag, double Percent) : ILenientGarnishmentBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientGarnishmentBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientGarnishmentBudgetAdjuster(List<double> Bounds, List<double> Rates) : ILenientGarnishmentBudgetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientGarnishmentBudget item, double valueValue)
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

public sealed record CapLenientGarnishmentBudgetAdjuster(double Cap) : ILenientGarnishmentBudgetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientGarnishmentBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLenientGarnishmentBudgetAdjuster(double Minimum) : ILenientGarnishmentBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientGarnishmentBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientGarnishmentBudgetAdjuster(double Percent) : ILenientGarnishmentBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientGarnishmentBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelLenientGarnishmentBudgetAdjuster(Dictionary<string, double> Factors) : ILenientGarnishmentBudgetAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientGarnishmentBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient garnishment budget steps in order and keeps a trace of each.</summary>
public sealed class LenientGarnishmentBudgetEngine(List<ILenientGarnishmentBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientGarnishmentBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientGarnishmentBudgetEngine DefaultLenientGarnishmentBudgetEngine(Registry flags)
    {
        return new LenientGarnishmentBudgetEngine(
            new List<ILenientGarnishmentBudgetAdjuster>
            {
                new FlaggedLenientGarnishmentBudgetAdjuster(flags, "lenient-garnishment-budget-manual", -2.6),
                new TieredLenientGarnishmentBudgetAdjuster(
                    new List<double> { 1828.0, 7191.0 },
                    new List<double> { 1.13, 1.07 }),
                new CapLenientGarnishmentBudgetAdjuster(42706.0),
                new MinimumLenientGarnishmentBudgetAdjuster(17.9),
                new PercentLenientGarnishmentBudgetAdjuster(19.0),
                new LabelLenientGarnishmentBudgetAdjuster(new Dictionary<string, double> { ["retail"] = 1.35 }),
            },
            new List<string>(),
            0.5,
            73467.0);
    }

    public double Compute(LenientGarnishmentBudget item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientGarnishmentBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
