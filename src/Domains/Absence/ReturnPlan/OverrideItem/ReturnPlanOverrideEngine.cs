namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan override calculation.</summary>
public interface IReturnPlanOverrideFactor
{
    string Describe();
    double Adjust(ReturnPlanOverride item, double valueValue);
}

public sealed record TieredReturnPlanOverrideAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanOverrideFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanOverride item, double valueValue)
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

public sealed record RoundingReturnPlanOverrideAdjuster(double Step) : IReturnPlanOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumReturnPlanOverrideAdjuster(double Minimum) : IReturnPlanOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ReturnPlanOverrideSequenceOffsetAdjuster(double Weight) : IReturnPlanOverrideFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(ReturnPlanOverride item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record PercentReturnPlanOverrideAdjuster(double Percent) : IReturnPlanOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan override steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanOverrideEngine(List<IReturnPlanOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanOverrideEngine DefaultReturnPlanOverrideEngine()
    {
        return new ReturnPlanOverrideEngine(
            new List<IReturnPlanOverrideFactor>
            {
                new TieredReturnPlanOverrideAdjuster(
                    new List<double> { 3890.0, 8091.0 },
                    new List<double> { 0.92, 0.83 }),
                new RoundingReturnPlanOverrideAdjuster(0.5),
                new MinimumReturnPlanOverrideAdjuster(36.1),
                new ReturnPlanOverrideSequenceOffsetAdjuster(1.41),
                new PercentReturnPlanOverrideAdjuster(8.1),
            },
            new List<string>(),
            3.1,
            58164.1);
    }

    public double Price(ReturnPlanOverride item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
