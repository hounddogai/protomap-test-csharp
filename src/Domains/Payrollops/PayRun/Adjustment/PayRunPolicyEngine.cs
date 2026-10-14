namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run policy calculation.</summary>
public interface IPayRunPolicyFactor
{
    string Describe();
    double Adjust(PayRunPolicy item, double valueValue);
}

public sealed record TieredPayRunPolicyAdjuster(List<double> Bounds, List<double> Rates) : IPayRunPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunPolicy item, double valueValue)
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

public sealed record RoundingPayRunPolicyAdjuster(double Step) : IPayRunPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocalePayRunPolicyAdjuster(Dictionary<string, double> Factors) : IPayRunPolicyFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PayRunPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPayRunPolicyAdjuster(double Minimum) : IPayRunPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPayRunPolicyAdjuster(double Cap) : IPayRunPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay run policy steps in order and keeps a trace of each.</summary>
public sealed class PayRunPolicyEngine(List<IPayRunPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunPolicyEngine DefaultPayRunPolicyEngine()
    {
        return new PayRunPolicyEngine(
            new List<IPayRunPolicyFactor>
            {
                new TieredPayRunPolicyAdjuster(new List<double> { 1387.0, 7197.0 }, new List<double> { 1.1, 0.89 }),
                new RoundingPayRunPolicyAdjuster(0.05),
                new LocalePayRunPolicyAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.92, ["west"] = 1.07 }),
                new MinimumPayRunPolicyAdjuster(22.6),
                new CapPayRunPolicyAdjuster(33294.0),
            },
            new List<string>(),
            4.3,
            51573.8);
    }

    public double Compute(PayRunPolicy item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
