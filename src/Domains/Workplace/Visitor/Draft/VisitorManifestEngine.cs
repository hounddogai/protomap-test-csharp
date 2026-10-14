namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor manifest calculation.</summary>
public interface IVisitorManifestAdjuster
{
    string Describe();
    double Adjust(VisitorManifest item, double valueValue);
}

public sealed record LocaleVisitorManifestAdjuster(Dictionary<string, double> Factors) : IVisitorManifestAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(VisitorManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumVisitorManifestAdjuster(double Minimum) : IVisitorManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredVisitorManifestAdjuster(List<double> Bounds, List<double> Rates) : IVisitorManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorManifest item, double valueValue)
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

/// <summary>Runs the visitor manifest steps in order and keeps a trace of each.</summary>
public sealed class VisitorManifestEngine(List<IVisitorManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorManifestEngine DefaultVisitorManifestEngine()
    {
        return new VisitorManifestEngine(
            new List<IVisitorManifestAdjuster>
            {
                new LocaleVisitorManifestAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.27, ["central"] = 1.18, ["west"] = 1.01 }),
                new MinimumVisitorManifestAdjuster(33.2),
                new TieredVisitorManifestAdjuster(
                    new List<double> { 381.0, 1075.0, 4892.0, 6283.0 },
                    new List<double> { 1.05, 1.04, 0.82, 1.18 }),
            },
            new List<string>(),
            4.5,
            78832.5);
    }

    public double Price(VisitorManifest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
