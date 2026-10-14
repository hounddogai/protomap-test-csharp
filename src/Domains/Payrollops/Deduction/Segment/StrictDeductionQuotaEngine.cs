namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict deduction quota calculation.</summary>
public interface IStrictDeductionQuotaStep
{
    string Describe();
    double Adjust(StrictDeductionQuota item, double valueValue);
}

public sealed record FlaggedStrictDeductionQuotaAdjuster(Registry Flags, string Flag, double Percent) : IStrictDeductionQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictDeductionQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictDeductionQuotaAdjuster(double Cap) : IStrictDeductionQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictDeductionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictDeductionQuotaAdjuster(double Step) : IStrictDeductionQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictDeductionQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictDeductionQuotaCeilingOffsetAdjuster(double Weight) : IStrictDeductionQuotaStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(StrictDeductionQuota item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the strict deduction quota steps in order and keeps a trace of each.</summary>
public sealed class StrictDeductionQuotaEngine(List<IStrictDeductionQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictDeductionQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictDeductionQuotaEngine DefaultStrictDeductionQuotaEngine(Registry flags)
    {
        return new StrictDeductionQuotaEngine(
            new List<IStrictDeductionQuotaStep>
            {
                new FlaggedStrictDeductionQuotaAdjuster(flags, "strict-deduction-quota-fallback", -0.0),
                new CapStrictDeductionQuotaAdjuster(9533.0),
                new RoundingStrictDeductionQuotaAdjuster(1.0),
                new StrictDeductionQuotaCeilingOffsetAdjuster(1.76),
            },
            new List<string>(),
            0.4,
            69211.5);
    }

    public double Evaluate(StrictDeductionQuota item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictDeductionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
