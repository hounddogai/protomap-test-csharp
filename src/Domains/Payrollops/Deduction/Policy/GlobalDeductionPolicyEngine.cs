namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global deduction policy calculation.</summary>
public interface IGlobalDeductionPolicyAdjuster
{
    string Describe();
    double Adjust(GlobalDeductionPolicy item, double valueValue);
}

public sealed record RoundingGlobalDeductionPolicyAdjuster(double Step) : IGlobalDeductionPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalDeductionPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedGlobalDeductionPolicyAdjuster(Registry Flags, string Flag, double Percent) : IGlobalDeductionPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalDeductionPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryGlobalDeductionPolicyAdjuster(Dictionary<string, double> Factors) : IGlobalDeductionPolicyAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GlobalDeductionPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredGlobalDeductionPolicyAdjuster(List<double> Bounds, List<double> Rates) : IGlobalDeductionPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalDeductionPolicy item, double valueValue)
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

/// <summary>Runs the global deduction policy steps in order and keeps a trace of each.</summary>
public sealed class GlobalDeductionPolicyEngine(List<IGlobalDeductionPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalDeductionPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalDeductionPolicyEngine DefaultGlobalDeductionPolicyEngine(Registry flags)
    {
        return new GlobalDeductionPolicyEngine(
            new List<IGlobalDeductionPolicyAdjuster>
            {
                new RoundingGlobalDeductionPolicyAdjuster(0.25),
                new FlaggedGlobalDeductionPolicyAdjuster(flags, "global-deduction-policy-global", 6.2),
                new CountryGlobalDeductionPolicyAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.93, ["retail"] = 1.03, ["online"] = 1.34 }),
                new TieredGlobalDeductionPolicyAdjuster(
                    new List<double> { 5175.0, 5553.0, 6882.0, 9252.0 },
                    new List<double> { 0.89, 1.02, 0.82, 0.91 }),
            },
            new List<string>(),
            1.9,
            80628.8);
    }

    public double Evaluate(GlobalDeductionPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalDeductionPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
