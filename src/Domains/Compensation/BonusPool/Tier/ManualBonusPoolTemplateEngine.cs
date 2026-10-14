namespace Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>One step of the manual bonus pool template calculation.</summary>
public interface IManualBonusPoolTemplateAdjuster
{
    string Describe();
    double Adjust(ManualBonusPoolTemplate item, double valueValue);
}

public sealed record PercentManualBonusPoolTemplateAdjuster(double Percent) : IManualBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualBonusPoolTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualBonusPoolTemplateAdjuster(double Minimum) : IManualBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualBonusPoolTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualBonusPoolTemplateAdjuster(double Cap) : IManualBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualBonusPoolTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualBonusPoolTemplateLimitOffsetAdjuster(double Weight) : IManualBonusPoolTemplateAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(ManualBonusPoolTemplate item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the manual bonus pool template steps in order and keeps a trace of each.</summary>
public sealed class ManualBonusPoolTemplateEngine(List<IManualBonusPoolTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBonusPoolTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBonusPoolTemplateEngine DefaultManualBonusPoolTemplateEngine()
    {
        return new ManualBonusPoolTemplateEngine(
            new List<IManualBonusPoolTemplateAdjuster>
            {
                new PercentManualBonusPoolTemplateAdjuster(4.4),
                new MinimumManualBonusPoolTemplateAdjuster(27.0),
                new CapManualBonusPoolTemplateAdjuster(45129.0),
                new ManualBonusPoolTemplateLimitOffsetAdjuster(1.38),
            },
            new List<string>(),
            3.8,
            98362.9);
    }

    public double Run(ManualBonusPoolTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualBonusPoolTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
