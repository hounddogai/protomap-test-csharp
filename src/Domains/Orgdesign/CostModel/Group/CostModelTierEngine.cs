namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model tier calculation.</summary>
public interface ICostModelTierFactor
{
    string Describe();
    double Adjust(CostModelTier item, double valueValue);
}

public sealed record ChannelCostModelTierAdjuster(Dictionary<string, double> Factors) : ICostModelTierFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CostModelTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCostModelTierAdjuster(double Cap) : ICostModelTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCostModelTierAdjuster(double Step) : ICostModelTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the cost model tier steps in order and keeps a trace of each.</summary>
public sealed class CostModelTierEngine(List<ICostModelTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelTierEngine DefaultCostModelTierEngine()
    {
        return new CostModelTierEngine(
            new List<ICostModelTierFactor>
            {
                new ChannelCostModelTierAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.11, ["retail"] = 1.29, ["east"] = 1.0 }),
                new CapCostModelTierAdjuster(32003.0),
                new RoundingCostModelTierAdjuster(1.0),
            },
            new List<string>(),
            4.7,
            63388.8);
    }

    public double Evaluate(CostModelTier item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
