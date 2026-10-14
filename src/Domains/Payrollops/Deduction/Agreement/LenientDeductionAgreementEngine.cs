namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient deduction agreement calculation.</summary>
public interface ILenientDeductionAgreementFactor
{
    string Describe();
    double Adjust(LenientDeductionAgreement item, double valueValue);
}

public sealed record LocaleLenientDeductionAgreementAdjuster(Dictionary<string, double> Factors) : ILenientDeductionAgreementFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LenientDeductionAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientDeductionAgreementAdjuster(double Cap) : ILenientDeductionAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientDeductionAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LenientDeductionAgreementAmountOffsetAdjuster(double Weight) : ILenientDeductionAgreementFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientDeductionAgreement item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record FlaggedLenientDeductionAgreementAdjuster(Registry Flags, string Flag, double Percent) : ILenientDeductionAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientDeductionAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientDeductionAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILenientDeductionAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientDeductionAgreement item, double valueValue)
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

/// <summary>Runs the lenient deduction agreement steps in order and keeps a trace of each.</summary>
public sealed class LenientDeductionAgreementEngine(List<ILenientDeductionAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientDeductionAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientDeductionAgreementEngine DefaultLenientDeductionAgreementEngine(Registry flags)
    {
        return new LenientDeductionAgreementEngine(
            new List<ILenientDeductionAgreementFactor>
            {
                new LocaleLenientDeductionAgreementAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.83, ["central"] = 1.02, ["south"] = 1.38 }),
                new CapLenientDeductionAgreementAdjuster(11375.0),
                new LenientDeductionAgreementAmountOffsetAdjuster(1.29),
                new FlaggedLenientDeductionAgreementAdjuster(flags, "lenient-deduction-agreement-manual", -4.1),
                new TieredLenientDeductionAgreementAdjuster(
                    new List<double> { 32.0, 5670.0 },
                    new List<double> { 1.08, 1.14 }),
            },
            new List<string>(),
            2.3,
            58221.4);
    }

    public double Run(LenientDeductionAgreement item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientDeductionAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
