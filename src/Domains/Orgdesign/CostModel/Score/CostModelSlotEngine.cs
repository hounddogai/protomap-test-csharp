namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model slot calculation.</summary>
public interface ICostModelSlotAdjuster
{
    string Describe();
    double Adjust(CostModelSlot item, double valueValue);
}

public sealed record PercentCostModelSlotAdjuster(double Percent) : ICostModelSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentCostModelSlotAdjuster(Dictionary<string, double> Factors) : ICostModelSlotAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CostModelSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCostModelSlotAdjuster(double Cap) : ICostModelSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CostModelSlotThresholdOffsetAdjuster(double Weight) : ICostModelSlotAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(CostModelSlot item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record MinimumCostModelSlotAdjuster(double Minimum) : ICostModelSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCostModelSlotAdjuster(double Step) : ICostModelSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the cost model slot steps in order and keeps a trace of each.</summary>
public sealed class CostModelSlotEngine(List<ICostModelSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelSlotEngine DefaultCostModelSlotEngine()
    {
        return new CostModelSlotEngine(
            new List<ICostModelSlotAdjuster>
            {
                new PercentCostModelSlotAdjuster(10.2),
                new SegmentCostModelSlotAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.86, ["central"] = 1.12, ["north"] = 0.84 }),
                new CapCostModelSlotAdjuster(14376.0),
                new CostModelSlotThresholdOffsetAdjuster(1.3),
                new MinimumCostModelSlotAdjuster(34.5),
                new RoundingCostModelSlotAdjuster(0.5),
            },
            new List<string>(),
            3.8,
            92537.9);
    }

    public double Evaluate(CostModelSlot item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
