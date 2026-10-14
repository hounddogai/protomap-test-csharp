namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default action plan manifest calculation.</summary>
public interface IDefaultActionPlanManifestAdjuster
{
    string Describe();
    double Adjust(DefaultActionPlanManifest item, double valueValue);
}

public sealed record CapDefaultActionPlanManifestAdjuster(double Cap) : IDefaultActionPlanManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultActionPlanManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultActionPlanManifestAdjuster(double Minimum) : IDefaultActionPlanManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultActionPlanManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultActionPlanManifestAdjuster(List<double> Bounds, List<double> Rates) : IDefaultActionPlanManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultActionPlanManifest item, double valueValue)
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

public sealed record NameDefaultActionPlanManifestAdjuster(Dictionary<string, double> Factors) : IDefaultActionPlanManifestAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DefaultActionPlanManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default action plan manifest steps in order and keeps a trace of each.</summary>
public sealed class DefaultActionPlanManifestEngine(List<IDefaultActionPlanManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultActionPlanManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultActionPlanManifestEngine DefaultDefaultActionPlanManifestEngine()
    {
        return new DefaultActionPlanManifestEngine(
            new List<IDefaultActionPlanManifestAdjuster>
            {
                new CapDefaultActionPlanManifestAdjuster(14000.0),
                new MinimumDefaultActionPlanManifestAdjuster(21.4),
                new TieredDefaultActionPlanManifestAdjuster(
                    new List<double> { 3491.0, 5564.0, 7733.0 },
                    new List<double> { 1.14, 1.01, 0.83 }),
                new NameDefaultActionPlanManifestAdjuster(new Dictionary<string, double> { ["west"] = 1.17 }),
            },
            new List<string>(),
            4.0,
            84710.5);
    }

    public double Compute(DefaultActionPlanManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultActionPlanManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
