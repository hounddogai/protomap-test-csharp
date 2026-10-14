namespace Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>One step of the refresh template calculation.</summary>
public interface IRefreshTemplateFactor
{
    string Describe();
    double Adjust(RefreshTemplate item, double valueValue);
}

public sealed record PercentRefreshTemplateAdjuster(double Percent) : IRefreshTemplateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRefreshTemplateAdjuster(List<double> Bounds, List<double> Rates) : IRefreshTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshTemplate item, double valueValue)
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

public sealed record CapRefreshTemplateAdjuster(double Cap) : IRefreshTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the refresh template steps in order and keeps a trace of each.</summary>
public sealed class RefreshTemplateEngine(List<IRefreshTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshTemplateEngine DefaultRefreshTemplateEngine()
    {
        return new RefreshTemplateEngine(
            new List<IRefreshTemplateFactor>
            {
                new PercentRefreshTemplateAdjuster(-21.2),
                new TieredRefreshTemplateAdjuster(
                    new List<double> { 638.0, 2360.0, 6148.0 },
                    new List<double> { 0.88, 1.06, 0.85 }),
                new CapRefreshTemplateAdjuster(19044.0),
            },
            new List<string>(),
            1.0,
            90791.0);
    }

    public double Evaluate(RefreshTemplate item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
