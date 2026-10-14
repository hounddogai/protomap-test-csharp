namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive pool draft calculation.</summary>
public interface IAdaptivePoolDraftStep
{
    string Describe();
    double Adjust(AdaptivePoolDraft item, double valueValue);
}

public sealed record PercentAdaptivePoolDraftAdjuster(double Percent) : IAdaptivePoolDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePoolDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptivePoolDraftAdjuster(double Minimum) : IAdaptivePoolDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePoolDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptivePoolDraftAdjuster(List<double> Bounds, List<double> Rates) : IAdaptivePoolDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptivePoolDraft item, double valueValue)
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

/// <summary>Runs the adaptive pool draft steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePoolDraftEngine(List<IAdaptivePoolDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePoolDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePoolDraftEngine DefaultAdaptivePoolDraftEngine()
    {
        return new AdaptivePoolDraftEngine(
            new List<IAdaptivePoolDraftStep>
            {
                new PercentAdaptivePoolDraftAdjuster(8.1),
                new MinimumAdaptivePoolDraftAdjuster(25.5),
                new TieredAdaptivePoolDraftAdjuster(
                    new List<double> { 322.0, 1745.0, 6525.0, 9075.0 },
                    new List<double> { 0.97, 0.94, 0.85, 0.81 }),
            },
            new List<string>(),
            4.7,
            87142.5);
    }

    public double Run(AdaptivePoolDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePoolDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
