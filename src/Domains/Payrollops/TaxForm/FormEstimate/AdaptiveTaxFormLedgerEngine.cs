namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive tax form ledger calculation.</summary>
public interface IAdaptiveTaxFormLedgerStep
{
    string Describe();
    double Adjust(AdaptiveTaxFormLedger item, double valueValue);
}

public sealed record RoundingAdaptiveTaxFormLedgerAdjuster(double Step) : IAdaptiveTaxFormLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTaxFormLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdaptiveTaxFormLedgerAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTaxFormLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTaxFormLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleAdaptiveTaxFormLedgerAdjuster(Dictionary<string, double> Factors) : IAdaptiveTaxFormLedgerStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AdaptiveTaxFormLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdaptiveTaxFormLedgerAdjuster(double Percent) : IAdaptiveTaxFormLedgerStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTaxFormLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveTaxFormLedgerDurationMinutesOffsetAdjuster(double Weight) : IAdaptiveTaxFormLedgerStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(AdaptiveTaxFormLedger item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the adaptive tax form ledger steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTaxFormLedgerEngine(List<IAdaptiveTaxFormLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTaxFormLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTaxFormLedgerEngine DefaultAdaptiveTaxFormLedgerEngine(Registry flags)
    {
        return new AdaptiveTaxFormLedgerEngine(
            new List<IAdaptiveTaxFormLedgerStep>
            {
                new RoundingAdaptiveTaxFormLedgerAdjuster(0.25),
                new FlaggedAdaptiveTaxFormLedgerAdjuster(flags, "adaptive-tax-form-ledger-weighted", -4.5),
                new LocaleAdaptiveTaxFormLedgerAdjuster(new Dictionary<string, double> { ["north"] = 1.05 }),
                new PercentAdaptiveTaxFormLedgerAdjuster(0.9),
                new AdaptiveTaxFormLedgerDurationMinutesOffsetAdjuster(1.4),
            },
            new List<string>(),
            0.1,
            23529.0);
    }

    public double Evaluate(AdaptiveTaxFormLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTaxFormLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
