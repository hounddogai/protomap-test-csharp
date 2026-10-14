namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global attestation bundle calculation.</summary>
public interface IGlobalAttestationBundleAdjuster
{
    string Describe();
    double Adjust(GlobalAttestationBundle item, double valueValue);
}

public sealed record TieredGlobalAttestationBundleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAttestationBundle item, double valueValue)
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

public sealed record PercentGlobalAttestationBundleAdjuster(double Percent) : IGlobalAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAttestationBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameGlobalAttestationBundleAdjuster(Dictionary<string, double> Factors) : IGlobalAttestationBundleAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalAttestationBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global attestation bundle steps in order and keeps a trace of each.</summary>
public sealed class GlobalAttestationBundleEngine(List<IGlobalAttestationBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAttestationBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAttestationBundleEngine DefaultGlobalAttestationBundleEngine()
    {
        return new GlobalAttestationBundleEngine(
            new List<IGlobalAttestationBundleAdjuster>
            {
                new TieredGlobalAttestationBundleAdjuster(
                    new List<double> { 3980.0, 7834.0 },
                    new List<double> { 1.18, 0.98 }),
                new PercentGlobalAttestationBundleAdjuster(20.0),
                new NameGlobalAttestationBundleAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.77, ["online"] = 1.05 }),
            },
            new List<string>(),
            3.3,
            44035.1);
    }

    public double Price(GlobalAttestationBundle item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAttestationBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
