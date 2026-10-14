namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient action plan record calculation.</summary>
public interface ILenientActionPlanRecordStep
{
    string Describe();
    double Adjust(LenientActionPlanRecord item, double valueValue);
}

public sealed record MinimumLenientActionPlanRecordAdjuster(double Minimum) : ILenientActionPlanRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientActionPlanRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameLenientActionPlanRecordAdjuster(Dictionary<string, double> Factors) : ILenientActionPlanRecordStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientActionPlanRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LenientActionPlanRecordHoursOffsetAdjuster(double Weight) : ILenientActionPlanRecordStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LenientActionPlanRecord item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record PercentLenientActionPlanRecordAdjuster(double Percent) : ILenientActionPlanRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientActionPlanRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientActionPlanRecordAdjuster(double Step) : ILenientActionPlanRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientActionPlanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient action plan record steps in order and keeps a trace of each.</summary>
public sealed class LenientActionPlanRecordEngine(List<ILenientActionPlanRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientActionPlanRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientActionPlanRecordEngine DefaultLenientActionPlanRecordEngine()
    {
        return new LenientActionPlanRecordEngine(
            new List<ILenientActionPlanRecordStep>
            {
                new MinimumLenientActionPlanRecordAdjuster(27.2),
                new NameLenientActionPlanRecordAdjuster(new Dictionary<string, double> { ["central"] = 0.74 }),
                new LenientActionPlanRecordHoursOffsetAdjuster(0.86),
                new PercentLenientActionPlanRecordAdjuster(22.6),
                new RoundingLenientActionPlanRecordAdjuster(0.25),
            },
            new List<string>(),
            2.8,
            39506.6);
    }

    public double Run(LenientActionPlanRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientActionPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
