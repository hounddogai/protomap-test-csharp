namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted engagement template calculation.</summary>
public interface IWeightedEngagementTemplateFactor
{
    string Describe();
    double Adjust(WeightedEngagementTemplate item, double valueValue);
}

public sealed record RoundingWeightedEngagementTemplateAdjuster(double Step) : IWeightedEngagementTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedEngagementTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapWeightedEngagementTemplateAdjuster(double Cap) : IWeightedEngagementTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedEngagementTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWeightedEngagementTemplateAdjuster(double Minimum) : IWeightedEngagementTemplateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedEngagementTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted engagement template steps in order and keeps a trace of each.</summary>
public sealed class WeightedEngagementTemplateEngine(List<IWeightedEngagementTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedEngagementTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedEngagementTemplateEngine DefaultWeightedEngagementTemplateEngine()
    {
        return new WeightedEngagementTemplateEngine(
            new List<IWeightedEngagementTemplateFactor>
            {
                new RoundingWeightedEngagementTemplateAdjuster(0.25),
                new CapWeightedEngagementTemplateAdjuster(46944.0),
                new MinimumWeightedEngagementTemplateAdjuster(46.1),
            },
            new List<string>(),
            0.3,
            30816.3);
    }

    public double Compute(WeightedEngagementTemplate item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedEngagementTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
