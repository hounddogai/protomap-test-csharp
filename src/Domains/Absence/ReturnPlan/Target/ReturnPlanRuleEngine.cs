namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan rule calculation.</summary>
public interface IReturnPlanRuleStep
{
    string Describe();
    double Adjust(ReturnPlanRule item, double valueValue);
}

public sealed record FlaggedReturnPlanRuleAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanRuleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ReturnPlanRuleVersionOffsetAdjuster(double Weight) : IReturnPlanRuleStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(ReturnPlanRule item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredReturnPlanRuleAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanRuleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanRule item, double valueValue)
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

public sealed record PercentReturnPlanRuleAdjuster(double Percent) : IReturnPlanRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReturnPlanRuleAdjuster(double Step) : IReturnPlanRuleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the return plan rule steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanRuleEngine(List<IReturnPlanRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanRuleEngine DefaultReturnPlanRuleEngine(Registry flags)
    {
        return new ReturnPlanRuleEngine(
            new List<IReturnPlanRuleStep>
            {
                new FlaggedReturnPlanRuleAdjuster(flags, "return-plan-rule-seasonal", 4.3),
                new ReturnPlanRuleVersionOffsetAdjuster(0.33),
                new TieredReturnPlanRuleAdjuster(
                    new List<double> { 5576.0, 8018.0 },
                    new List<double> { 0.94, 0.96 }),
                new PercentReturnPlanRuleAdjuster(-0.8),
                new RoundingReturnPlanRuleAdjuster(0.1),
            },
            new List<string>(),
            3.3,
            6261.4);
    }

    public double Price(ReturnPlanRule item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
