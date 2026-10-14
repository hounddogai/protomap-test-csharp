namespace Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>One step of the lenient return plan override calculation.</summary>
public interface ILenientReturnPlanOverrideModifier
{
    string Describe();
    double Adjust(LenientReturnPlanOverride item, double valueValue);
}

public sealed record TieredLenientReturnPlanOverrideAdjuster(List<double> Bounds, List<double> Rates) : ILenientReturnPlanOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientReturnPlanOverride item, double valueValue)
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

public sealed record CapLenientReturnPlanOverrideAdjuster(double Cap) : ILenientReturnPlanOverrideModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientReturnPlanOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLenientReturnPlanOverrideAdjuster(double Minimum) : ILenientReturnPlanOverrideModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientReturnPlanOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient return plan override steps in order and keeps a trace of each.</summary>
public sealed class LenientReturnPlanOverrideEngine(List<ILenientReturnPlanOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientReturnPlanOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientReturnPlanOverrideEngine DefaultLenientReturnPlanOverrideEngine()
    {
        return new LenientReturnPlanOverrideEngine(
            new List<ILenientReturnPlanOverrideModifier>
            {
                new TieredLenientReturnPlanOverrideAdjuster(
                    new List<double> { 4895.0, 5046.0, 5624.0 },
                    new List<double> { 1.18, 0.83, 1.13 }),
                new CapLenientReturnPlanOverrideAdjuster(15487.0),
                new MinimumLenientReturnPlanOverrideAdjuster(20.3),
            },
            new List<string>(),
            4.0,
            47872.0);
    }

    public double Price(LenientReturnPlanOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientReturnPlanOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
