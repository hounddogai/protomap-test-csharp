namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment agreement calculation.</summary>
public interface IAdjustmentAgreementFactor
{
    string Describe();
    double Adjust(AdjustmentAgreement item, double valueValue);
}

public sealed record AdjustmentAgreementRetriesOffsetAdjuster(double Weight) : IAdjustmentAgreementFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AdjustmentAgreement item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedAdjustmentAgreementAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdjustmentAgreementAdjuster(double Percent) : IAdjustmentAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment agreement steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentAgreementEngine(List<IAdjustmentAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentAgreementEngine DefaultAdjustmentAgreementEngine(Registry flags)
    {
        return new AdjustmentAgreementEngine(
            new List<IAdjustmentAgreementFactor>
            {
                new AdjustmentAgreementRetriesOffsetAdjuster(1.72),
                new FlaggedAdjustmentAgreementAdjuster(flags, "adjustment-agreement-tiered", -7.7),
                new PercentAdjustmentAgreementAdjuster(-21.9),
            },
            new List<string>(),
            4.5,
            8844.9);
    }

    public double Run(AdjustmentAgreement item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
