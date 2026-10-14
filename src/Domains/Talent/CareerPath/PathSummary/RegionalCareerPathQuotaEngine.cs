namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional career path quota calculation.</summary>
public interface IRegionalCareerPathQuotaAdjuster
{
    string Describe();
    double Adjust(RegionalCareerPathQuota item, double valueValue);
}

public sealed record PercentRegionalCareerPathQuotaAdjuster(double Percent) : IRegionalCareerPathQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalCareerPathQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalCareerPathQuotaAdjuster(double Cap) : IRegionalCareerPathQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalCareerPathQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRegionalCareerPathQuotaAdjuster(List<double> Bounds, List<double> Rates) : IRegionalCareerPathQuotaAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalCareerPathQuota item, double valueValue)
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

public sealed record RoundingRegionalCareerPathQuotaAdjuster(double Step) : IRegionalCareerPathQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalCareerPathQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalCareerPathQuotaAdjuster(double Minimum) : IRegionalCareerPathQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalCareerPathQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional career path quota steps in order and keeps a trace of each.</summary>
public sealed class RegionalCareerPathQuotaEngine(List<IRegionalCareerPathQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalCareerPathQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalCareerPathQuotaEngine DefaultRegionalCareerPathQuotaEngine()
    {
        return new RegionalCareerPathQuotaEngine(
            new List<IRegionalCareerPathQuotaAdjuster>
            {
                new PercentRegionalCareerPathQuotaAdjuster(-13.0),
                new CapRegionalCareerPathQuotaAdjuster(49062.0),
                new TieredRegionalCareerPathQuotaAdjuster(
                    new List<double> { 4251.0, 4853.0, 5977.0, 8486.0 },
                    new List<double> { 0.81, 0.88, 1.18, 1.1 }),
                new RoundingRegionalCareerPathQuotaAdjuster(0.1),
                new MinimumRegionalCareerPathQuotaAdjuster(21.8),
            },
            new List<string>(),
            0.3,
            36005.9);
    }

    public double Run(RegionalCareerPathQuota item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalCareerPathQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
