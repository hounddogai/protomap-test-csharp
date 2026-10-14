namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled refresh mapping calculation.</summary>
public interface IScheduledRefreshMappingStep
{
    string Describe();
    double Adjust(ScheduledRefreshMapping item, double valueValue);
}

public sealed record TeamScheduledRefreshMappingAdjuster(Dictionary<string, double> Factors) : IScheduledRefreshMappingStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ScheduledRefreshMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScheduledRefreshMappingMinimumOffsetAdjuster(double Weight) : IScheduledRefreshMappingStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ScheduledRefreshMapping item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CapScheduledRefreshMappingAdjuster(double Cap) : IScheduledRefreshMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledRefreshMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledRefreshMappingAdjuster(double Minimum) : IScheduledRefreshMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledRefreshMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScheduledRefreshMappingAdjuster(double Step) : IScheduledRefreshMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledRefreshMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScheduledRefreshMappingAdjuster(double Percent) : IScheduledRefreshMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledRefreshMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled refresh mapping steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRefreshMappingEngine(List<IScheduledRefreshMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRefreshMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRefreshMappingEngine DefaultScheduledRefreshMappingEngine()
    {
        return new ScheduledRefreshMappingEngine(
            new List<IScheduledRefreshMappingStep>
            {
                new TeamScheduledRefreshMappingAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.97, ["retail"] = 1.21, ["west"] = 1.37 }),
                new ScheduledRefreshMappingMinimumOffsetAdjuster(1.86),
                new CapScheduledRefreshMappingAdjuster(18902.0),
                new MinimumScheduledRefreshMappingAdjuster(35.8),
                new RoundingScheduledRefreshMappingAdjuster(0.1),
                new PercentScheduledRefreshMappingAdjuster(-16.9),
            },
            new List<string>(),
            2.5,
            72888.8);
    }

    public double Compute(ScheduledRefreshMapping item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRefreshMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
