namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global pay band agreement calculation.</summary>
public interface IGlobalPayBandAgreementFactor
{
    string Describe();
    double Adjust(GlobalPayBandAgreement item, double valueValue);
}

public sealed record TieredGlobalPayBandAgreementAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPayBandAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPayBandAgreement item, double valueValue)
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

public sealed record RoundingGlobalPayBandAgreementAdjuster(double Step) : IGlobalPayBandAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPayBandAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelGlobalPayBandAgreementAdjuster(Dictionary<string, double> Factors) : IGlobalPayBandAgreementFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalPayBandAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global pay band agreement steps in order and keeps a trace of each.</summary>
public sealed class GlobalPayBandAgreementEngine(List<IGlobalPayBandAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPayBandAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPayBandAgreementEngine DefaultGlobalPayBandAgreementEngine()
    {
        return new GlobalPayBandAgreementEngine(
            new List<IGlobalPayBandAgreementFactor>
            {
                new TieredGlobalPayBandAgreementAdjuster(
                    new List<double> { 821.0, 3946.0, 5167.0, 9686.0 },
                    new List<double> { 0.98, 1.11, 1.15, 1.13 }),
                new RoundingGlobalPayBandAgreementAdjuster(0.25),
                new LabelGlobalPayBandAgreementAdjuster(new Dictionary<string, double> { ["north"] = 1.07 }),
            },
            new List<string>(),
            0.5,
            8278.2);
    }

    public double Run(GlobalPayBandAgreement item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPayBandAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
