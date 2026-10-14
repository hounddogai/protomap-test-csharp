namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict accrual template calculation.</summary>
public interface IStrictAccrualTemplateStep
{
    string Describe();
    double Adjust(StrictAccrualTemplate item, double valueValue);
}

public sealed record StrictAccrualTemplateMaximumOffsetAdjuster(double Weight) : IStrictAccrualTemplateStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(StrictAccrualTemplate item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record RoundingStrictAccrualTemplateAdjuster(double Step) : IStrictAccrualTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAccrualTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceStrictAccrualTemplateAdjuster(Dictionary<string, double> Factors) : IStrictAccrualTemplateStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(StrictAccrualTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictAccrualTemplateAdjuster(double Minimum) : IStrictAccrualTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictAccrualTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict accrual template steps in order and keeps a trace of each.</summary>
public sealed class StrictAccrualTemplateEngine(List<IStrictAccrualTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAccrualTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAccrualTemplateEngine DefaultStrictAccrualTemplateEngine()
    {
        return new StrictAccrualTemplateEngine(
            new List<IStrictAccrualTemplateStep>
            {
                new StrictAccrualTemplateMaximumOffsetAdjuster(1.22),
                new RoundingStrictAccrualTemplateAdjuster(0.5),
                new SourceStrictAccrualTemplateAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.3, ["east"] = 1.11 }),
                new MinimumStrictAccrualTemplateAdjuster(40.0),
            },
            new List<string>(),
            4.1,
            26718.2);
    }

    public double Run(StrictAccrualTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictAccrualTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
