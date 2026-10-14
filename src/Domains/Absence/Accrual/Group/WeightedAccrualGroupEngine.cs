namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted accrual group calculation.</summary>
public interface IWeightedAccrualGroupModifier
{
    string Describe();
    double Adjust(WeightedAccrualGroup item, double valueValue);
}

public sealed record MinimumWeightedAccrualGroupAdjuster(double Minimum) : IWeightedAccrualGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAccrualGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedAccrualGroupAdjuster(double Percent) : IWeightedAccrualGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAccrualGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleWeightedAccrualGroupAdjuster(Dictionary<string, double> Factors) : IWeightedAccrualGroupModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WeightedAccrualGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWeightedAccrualGroupAdjuster(double Step) : IWeightedAccrualGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAccrualGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapWeightedAccrualGroupAdjuster(double Cap) : IWeightedAccrualGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAccrualGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted accrual group steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccrualGroupEngine(List<IWeightedAccrualGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccrualGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccrualGroupEngine DefaultWeightedAccrualGroupEngine()
    {
        return new WeightedAccrualGroupEngine(
            new List<IWeightedAccrualGroupModifier>
            {
                new MinimumWeightedAccrualGroupAdjuster(44.5),
                new PercentWeightedAccrualGroupAdjuster(-13.8),
                new TitleWeightedAccrualGroupAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.01, ["north"] = 1.16, ["retail"] = 1.38 }),
                new RoundingWeightedAccrualGroupAdjuster(5.0),
                new CapWeightedAccrualGroupAdjuster(15579.0),
            },
            new List<string>(),
            0.6,
            28930.7);
    }

    public double Evaluate(WeightedAccrualGroup item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccrualGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
