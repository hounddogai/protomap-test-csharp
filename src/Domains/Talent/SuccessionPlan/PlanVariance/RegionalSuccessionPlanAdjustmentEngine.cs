namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional succession plan adjustment calculation.</summary>
public interface IRegionalSuccessionPlanAdjustmentModifier
{
    string Describe();
    double Adjust(RegionalSuccessionPlanAdjustment item, double valueValue);
}

public sealed record RoundingRegionalSuccessionPlanAdjustmentAdjuster(double Step) : IRegionalSuccessionPlanAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSuccessionPlanAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelRegionalSuccessionPlanAdjustmentAdjuster(Dictionary<string, double> Factors) : IRegionalSuccessionPlanAdjustmentModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RegionalSuccessionPlanAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalSuccessionPlanAdjustmentAmountOffsetAdjuster(double Weight) : IRegionalSuccessionPlanAdjustmentModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RegionalSuccessionPlanAdjustment item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the regional succession plan adjustment steps in order and keeps a trace of each.</summary>
public sealed class RegionalSuccessionPlanAdjustmentEngine(List<IRegionalSuccessionPlanAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSuccessionPlanAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSuccessionPlanAdjustmentEngine DefaultRegionalSuccessionPlanAdjustmentEngine()
    {
        return new RegionalSuccessionPlanAdjustmentEngine(
            new List<IRegionalSuccessionPlanAdjustmentModifier>
            {
                new RoundingRegionalSuccessionPlanAdjustmentAdjuster(0.05),
                new ChannelRegionalSuccessionPlanAdjustmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.96, ["east"] = 1.25, ["online"] = 1.0 }),
                new RegionalSuccessionPlanAdjustmentAmountOffsetAdjuster(1.97),
            },
            new List<string>(),
            2.1,
            49964.7);
    }

    public double Price(RegionalSuccessionPlanAdjustment item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSuccessionPlanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
