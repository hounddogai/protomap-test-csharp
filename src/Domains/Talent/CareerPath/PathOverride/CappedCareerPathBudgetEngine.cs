namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped career path budget calculation.</summary>
public interface ICappedCareerPathBudgetModifier
{
    string Describe();
    double Adjust(CappedCareerPathBudget item, double valueValue);
}

public sealed record TieredCappedCareerPathBudgetAdjuster(List<double> Bounds, List<double> Rates) : ICappedCareerPathBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedCareerPathBudget item, double valueValue)
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

public sealed record LabelCappedCareerPathBudgetAdjuster(Dictionary<string, double> Factors) : ICappedCareerPathBudgetModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedCareerPathBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CappedCareerPathBudgetTenureMonthsOffsetAdjuster(double Weight) : ICappedCareerPathBudgetModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(CappedCareerPathBudget item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record CapCappedCareerPathBudgetAdjuster(double Cap) : ICappedCareerPathBudgetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedCareerPathBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedCareerPathBudgetAdjuster(Registry Flags, string Flag, double Percent) : ICappedCareerPathBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedCareerPathBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped career path budget steps in order and keeps a trace of each.</summary>
public sealed class CappedCareerPathBudgetEngine(List<ICappedCareerPathBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCareerPathBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCareerPathBudgetEngine DefaultCappedCareerPathBudgetEngine(Registry flags)
    {
        return new CappedCareerPathBudgetEngine(
            new List<ICappedCareerPathBudgetModifier>
            {
                new TieredCappedCareerPathBudgetAdjuster(
                    new List<double> { 846.0, 2080.0 },
                    new List<double> { 0.89, 0.88 }),
                new LabelCappedCareerPathBudgetAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.87, ["south"] = 1.01, ["west"] = 0.88 }),
                new CappedCareerPathBudgetTenureMonthsOffsetAdjuster(1.4),
                new CapCappedCareerPathBudgetAdjuster(13190.0),
                new FlaggedCappedCareerPathBudgetAdjuster(flags, "capped-career-path-budget-seasonal", 3.0),
            },
            new List<string>(),
            2.0,
            99722.6);
    }

    public double Run(CappedCareerPathBudget item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCareerPathBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
