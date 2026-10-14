namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary return plan manifest calculation.</summary>
public interface IPrimaryReturnPlanManifestStep
{
    string Describe();
    double Adjust(PrimaryReturnPlanManifest item, double valueValue);
}

public sealed record MinimumPrimaryReturnPlanManifestAdjuster(double Minimum) : IPrimaryReturnPlanManifestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryReturnPlanManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPrimaryReturnPlanManifestAdjuster(double Percent) : IPrimaryReturnPlanManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryReturnPlanManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryReturnPlanManifestAdjuster(double Cap) : IPrimaryReturnPlanManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryReturnPlanManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitlePrimaryReturnPlanManifestAdjuster(Dictionary<string, double> Factors) : IPrimaryReturnPlanManifestStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryReturnPlanManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPrimaryReturnPlanManifestAdjuster(double Step) : IPrimaryReturnPlanManifestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryReturnPlanManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PrimaryReturnPlanManifestExpiresAtOffsetAdjuster(double Weight) : IPrimaryReturnPlanManifestStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PrimaryReturnPlanManifest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the primary return plan manifest steps in order and keeps a trace of each.</summary>
public sealed class PrimaryReturnPlanManifestEngine(List<IPrimaryReturnPlanManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryReturnPlanManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryReturnPlanManifestEngine DefaultPrimaryReturnPlanManifestEngine()
    {
        return new PrimaryReturnPlanManifestEngine(
            new List<IPrimaryReturnPlanManifestStep>
            {
                new MinimumPrimaryReturnPlanManifestAdjuster(8.6),
                new PercentPrimaryReturnPlanManifestAdjuster(21.6),
                new CapPrimaryReturnPlanManifestAdjuster(10877.0),
                new TitlePrimaryReturnPlanManifestAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.39, ["retail"] = 1.05 }),
                new RoundingPrimaryReturnPlanManifestAdjuster(0.5),
                new PrimaryReturnPlanManifestExpiresAtOffsetAdjuster(0.36),
            },
            new List<string>(),
            2.8,
            63022.9);
    }

    public double Run(PrimaryReturnPlanManifest item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryReturnPlanManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
