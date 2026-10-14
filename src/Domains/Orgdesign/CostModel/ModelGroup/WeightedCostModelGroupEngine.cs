namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted cost model group calculation.</summary>
public interface IWeightedCostModelGroupModifier
{
    string Describe();
    double Adjust(WeightedCostModelGroup item, double valueValue);
}

public sealed record FlaggedWeightedCostModelGroupAdjuster(Registry Flags, string Flag, double Percent) : IWeightedCostModelGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedCostModelGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedCostModelGroupAdjuster(double Cap) : IWeightedCostModelGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedCostModelGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryWeightedCostModelGroupAdjuster(Dictionary<string, double> Factors) : IWeightedCostModelGroupModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(WeightedCostModelGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedCostModelGroupAdjuster(double Minimum) : IWeightedCostModelGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedCostModelGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted cost model group steps in order and keeps a trace of each.</summary>
public sealed class WeightedCostModelGroupEngine(List<IWeightedCostModelGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedCostModelGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedCostModelGroupEngine DefaultWeightedCostModelGroupEngine(Registry flags)
    {
        return new WeightedCostModelGroupEngine(
            new List<IWeightedCostModelGroupModifier>
            {
                new FlaggedWeightedCostModelGroupAdjuster(flags, "weighted-cost-model-group-strict", -0.6),
                new CapWeightedCostModelGroupAdjuster(42438.0),
                new CountryWeightedCostModelGroupAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.27, ["north"] = 0.98, ["online"] = 0.72 }),
                new MinimumWeightedCostModelGroupAdjuster(46.4),
            },
            new List<string>(),
            3.1,
            22867.0);
    }

    public double Compute(WeightedCostModelGroup item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedCostModelGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
