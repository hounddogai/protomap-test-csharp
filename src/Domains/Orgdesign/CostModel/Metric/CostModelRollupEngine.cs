namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model rollup calculation.</summary>
public interface ICostModelRollupModifier
{
    string Describe();
    double Adjust(CostModelRollup item, double valueValue);
}

public sealed record PercentCostModelRollupAdjuster(double Percent) : ICostModelRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCostModelRollupAdjuster(double Minimum) : ICostModelRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCostModelRollupAdjuster(double Step) : ICostModelRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelCostModelRollupAdjuster(Dictionary<string, double> Factors) : ICostModelRollupModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CostModelRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the cost model rollup steps in order and keeps a trace of each.</summary>
public sealed class CostModelRollupEngine(List<ICostModelRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelRollupEngine DefaultCostModelRollupEngine()
    {
        return new CostModelRollupEngine(
            new List<ICostModelRollupModifier>
            {
                new PercentCostModelRollupAdjuster(-16.3),
                new MinimumCostModelRollupAdjuster(21.1),
                new RoundingCostModelRollupAdjuster(0.1),
                new ChannelCostModelRollupAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.14, ["central"] = 0.73 }),
            },
            new List<string>(),
            1.1,
            21041.7);
    }

    public double Run(CostModelRollup item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
