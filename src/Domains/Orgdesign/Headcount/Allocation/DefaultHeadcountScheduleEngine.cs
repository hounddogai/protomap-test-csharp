namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default headcount schedule calculation.</summary>
public interface IDefaultHeadcountScheduleStep
{
    string Describe();
    double Adjust(DefaultHeadcountSchedule item, double valueValue);
}

public sealed record TitleDefaultHeadcountScheduleAdjuster(Dictionary<string, double> Factors) : IDefaultHeadcountScheduleStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultHeadcountSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultHeadcountScheduleAdjuster(double Step) : IDefaultHeadcountScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultHeadcountSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultHeadcountScheduleAdjuster(double Minimum) : IDefaultHeadcountScheduleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultHeadcountSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultHeadcountScheduleAdjuster(double Percent) : IDefaultHeadcountScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultHeadcountSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default headcount schedule steps in order and keeps a trace of each.</summary>
public sealed class DefaultHeadcountScheduleEngine(List<IDefaultHeadcountScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultHeadcountScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultHeadcountScheduleEngine DefaultDefaultHeadcountScheduleEngine()
    {
        return new DefaultHeadcountScheduleEngine(
            new List<IDefaultHeadcountScheduleStep>
            {
                new TitleDefaultHeadcountScheduleAdjuster(new Dictionary<string, double> { ["east"] = 1.18 }),
                new RoundingDefaultHeadcountScheduleAdjuster(0.5),
                new MinimumDefaultHeadcountScheduleAdjuster(20.9),
                new PercentDefaultHeadcountScheduleAdjuster(23.9),
            },
            new List<string>(),
            3.5,
            22294.5);
    }

    public double Run(DefaultHeadcountSchedule item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultHeadcountSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
