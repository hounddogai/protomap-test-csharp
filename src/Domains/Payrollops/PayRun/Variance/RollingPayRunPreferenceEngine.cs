namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling pay run preference calculation.</summary>
public interface IRollingPayRunPreferenceStep
{
    string Describe();
    double Adjust(RollingPayRunPreference item, double valueValue);
}

public sealed record RoundingRollingPayRunPreferenceAdjuster(double Step) : IRollingPayRunPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPayRunPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRollingPayRunPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IRollingPayRunPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPayRunPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingPayRunPreferenceDiscountOffsetAdjuster(double Weight) : IRollingPayRunPreferenceStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(RollingPayRunPreference item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TieredRollingPayRunPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IRollingPayRunPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingPayRunPreference item, double valueValue)
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

/// <summary>Runs the rolling pay run preference steps in order and keeps a trace of each.</summary>
public sealed class RollingPayRunPreferenceEngine(List<IRollingPayRunPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPayRunPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPayRunPreferenceEngine DefaultRollingPayRunPreferenceEngine(Registry flags)
    {
        return new RollingPayRunPreferenceEngine(
            new List<IRollingPayRunPreferenceStep>
            {
                new RoundingRollingPayRunPreferenceAdjuster(0.1),
                new FlaggedRollingPayRunPreferenceAdjuster(flags, "rolling-pay-run-preference-primary", -9.1),
                new RollingPayRunPreferenceDiscountOffsetAdjuster(1.51),
                new TieredRollingPayRunPreferenceAdjuster(
                    new List<double> { 3704.0, 4083.0 },
                    new List<double> { 1.18, 0.8 }),
            },
            new List<string>(),
            1.9,
            30382.0);
    }

    public double Evaluate(RollingPayRunPreference item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingPayRunPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
