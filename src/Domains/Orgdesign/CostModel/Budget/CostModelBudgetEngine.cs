namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model budget calculation.</summary>
public interface ICostModelBudgetFactor
{
    string Describe();
    double Adjust(CostModelBudget item, double valueValue);
}

public sealed record CountryCostModelBudgetAdjuster(Dictionary<string, double> Factors) : ICostModelBudgetFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(CostModelBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCostModelBudgetAdjuster(double Percent) : ICostModelBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCostModelBudgetAdjuster(double Minimum) : ICostModelBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCostModelBudgetAdjuster(double Step) : ICostModelBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CostModelBudgetEndsAtOffsetAdjuster(double Weight) : ICostModelBudgetFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(CostModelBudget item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CapCostModelBudgetAdjuster(double Cap) : ICostModelBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the cost model budget steps in order and keeps a trace of each.</summary>
public sealed class CostModelBudgetEngine(List<ICostModelBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelBudgetEngine DefaultCostModelBudgetEngine()
    {
        return new CostModelBudgetEngine(
            new List<ICostModelBudgetFactor>
            {
                new CountryCostModelBudgetAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.13, ["retail"] = 0.82, ["central"] = 0.92 }),
                new PercentCostModelBudgetAdjuster(24.3),
                new MinimumCostModelBudgetAdjuster(43.3),
                new RoundingCostModelBudgetAdjuster(0.05),
                new CostModelBudgetEndsAtOffsetAdjuster(1.79),
                new CapCostModelBudgetAdjuster(19062.0),
            },
            new List<string>(),
            3.4,
            87424.3);
    }

    public double Compute(CostModelBudget item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
