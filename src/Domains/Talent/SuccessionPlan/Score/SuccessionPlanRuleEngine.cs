namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan rule calculation.</summary>
public interface ISuccessionPlanRuleAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanRule item, double valueValue);
}

public sealed record PercentSuccessionPlanRuleAdjuster(double Percent) : ISuccessionPlanRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentSuccessionPlanRuleAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanRuleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SuccessionPlanRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSuccessionPlanRuleAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanRule item, double valueValue)
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

public sealed record CapSuccessionPlanRuleAdjuster(double Cap) : ISuccessionPlanRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SuccessionPlanRuleLimitOffsetAdjuster(double Weight) : ISuccessionPlanRuleAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(SuccessionPlanRule item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the succession plan rule steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanRuleEngine(List<ISuccessionPlanRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanRuleEngine DefaultSuccessionPlanRuleEngine()
    {
        return new SuccessionPlanRuleEngine(
            new List<ISuccessionPlanRuleAdjuster>
            {
                new PercentSuccessionPlanRuleAdjuster(18.2),
                new DepartmentSuccessionPlanRuleAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.87, ["north"] = 1.08 }),
                new TieredSuccessionPlanRuleAdjuster(
                    new List<double> { 2385.0, 2518.0, 5624.0, 7482.0 },
                    new List<double> { 1.0, 0.83, 1.12, 0.88 }),
                new CapSuccessionPlanRuleAdjuster(41432.0),
                new SuccessionPlanRuleLimitOffsetAdjuster(0.77),
            },
            new List<string>(),
            0.0,
            60108.9);
    }

    public double Run(SuccessionPlanRule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
