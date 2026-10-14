namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped deduction policy calculation.</summary>
public interface ICappedDeductionPolicyStep
{
    string Describe();
    double Adjust(CappedDeductionPolicy item, double valueValue);
}

public sealed record FlaggedCappedDeductionPolicyAdjuster(Registry Flags, string Flag, double Percent) : ICappedDeductionPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedDeductionPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedDeductionPolicyWindowDaysOffsetAdjuster(double Weight) : ICappedDeductionPolicyStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(CappedDeductionPolicy item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumCappedDeductionPolicyAdjuster(double Minimum) : ICappedDeductionPolicyStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedDeductionPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedDeductionPolicyAdjuster(List<double> Bounds, List<double> Rates) : ICappedDeductionPolicyStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedDeductionPolicy item, double valueValue)
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

public sealed record RoundingCappedDeductionPolicyAdjuster(double Step) : ICappedDeductionPolicyStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedDeductionPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCappedDeductionPolicyAdjuster(double Cap) : ICappedDeductionPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedDeductionPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped deduction policy steps in order and keeps a trace of each.</summary>
public sealed class CappedDeductionPolicyEngine(List<ICappedDeductionPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedDeductionPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedDeductionPolicyEngine DefaultCappedDeductionPolicyEngine(Registry flags)
    {
        return new CappedDeductionPolicyEngine(
            new List<ICappedDeductionPolicyStep>
            {
                new FlaggedCappedDeductionPolicyAdjuster(flags, "capped-deduction-policy-weighted", -1.5),
                new CappedDeductionPolicyWindowDaysOffsetAdjuster(0.4),
                new MinimumCappedDeductionPolicyAdjuster(39.2),
                new TieredCappedDeductionPolicyAdjuster(
                    new List<double> { 68.0, 914.0, 2723.0, 3408.0 },
                    new List<double> { 0.96, 1.03, 1.11, 0.99 }),
                new RoundingCappedDeductionPolicyAdjuster(0.1),
                new CapCappedDeductionPolicyAdjuster(14059.0),
            },
            new List<string>(),
            4.2,
            37525.8);
    }

    public double Price(CappedDeductionPolicy item)
    {
        var current = item.EffectiveExposure();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CappedDeductionPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
