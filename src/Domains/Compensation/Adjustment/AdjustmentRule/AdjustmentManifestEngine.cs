namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment manifest calculation.</summary>
public interface IAdjustmentManifestStep
{
    string Describe();
    double Adjust(AdjustmentManifest item, double valueValue);
}

public sealed record TieredAdjustmentManifestAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentManifestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentManifest item, double valueValue)
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

public sealed record PercentAdjustmentManifestAdjuster(double Percent) : IAdjustmentManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdjustmentManifestAdjuster(double Minimum) : IAdjustmentManifestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentManifestAdjuster(double Cap) : IAdjustmentManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleAdjustmentManifestAdjuster(Dictionary<string, double> Factors) : IAdjustmentManifestStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdjustmentManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdjustmentManifestAdjuster(double Step) : IAdjustmentManifestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adjustment manifest steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentManifestEngine(List<IAdjustmentManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentManifestEngine DefaultAdjustmentManifestEngine()
    {
        return new AdjustmentManifestEngine(
            new List<IAdjustmentManifestStep>
            {
                new TieredAdjustmentManifestAdjuster(
                    new List<double> { 2184.0, 3365.0, 7468.0 },
                    new List<double> { 1.19, 0.92, 1.19 }),
                new PercentAdjustmentManifestAdjuster(22.3),
                new MinimumAdjustmentManifestAdjuster(38.0),
                new CapAdjustmentManifestAdjuster(14991.0),
                new TitleAdjustmentManifestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.24, ["central"] = 1.34 }),
                new RoundingAdjustmentManifestAdjuster(0.05),
            },
            new List<string>(),
            3.3,
            1225.0);
    }

    public double Run(AdjustmentManifest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
