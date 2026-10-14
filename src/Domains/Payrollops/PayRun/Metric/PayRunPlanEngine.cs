namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay run plan calculation.</summary>
public interface IPayRunPlanFactor
{
    string Describe();
    double Adjust(PayRunPlan item, double valueValue);
}

public sealed record CapPayRunPlanAdjuster(double Cap) : IPayRunPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PayRunPlanLimitOffsetAdjuster(double Weight) : IPayRunPlanFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PayRunPlan item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredPayRunPlanAdjuster(List<double> Bounds, List<double> Rates) : IPayRunPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunPlan item, double valueValue)
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

public sealed record PercentPayRunPlanAdjuster(double Percent) : IPayRunPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPayRunPlanAdjuster(Registry Flags, string Flag, double Percent) : IPayRunPlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitPayRunPlanAdjuster(Dictionary<string, double> Factors) : IPayRunPlanFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PayRunPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay run plan steps in order and keeps a trace of each.</summary>
public sealed class PayRunPlanEngine(List<IPayRunPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunPlanEngine DefaultPayRunPlanEngine(Registry flags)
    {
        return new PayRunPlanEngine(
            new List<IPayRunPlanFactor>
            {
                new CapPayRunPlanAdjuster(48518.0),
                new PayRunPlanLimitOffsetAdjuster(0.23),
                new TieredPayRunPlanAdjuster(new List<double> { 2271.0, 9405.0 }, new List<double> { 1.03, 1.02 }),
                new PercentPayRunPlanAdjuster(-18.7),
                new FlaggedPayRunPlanAdjuster(flags, "pay-run-plan-rolling", -0.2),
                new UnitPayRunPlanAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.25, ["retail"] = 0.72, ["west"] = 0.85 }),
            },
            new List<string>(),
            0.2,
            2159.8);
    }

    public double Run(PayRunPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
