namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding baseline calculation.</summary>
public interface IOffboardingBaselineAdjuster
{
    string Describe();
    double Adjust(OffboardingBaseline item, double valueValue);
}

public sealed record RoundingOffboardingBaselineAdjuster(double Step) : IOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleOffboardingBaselineAdjuster(Dictionary<string, double> Factors) : IOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(OffboardingBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentOffboardingBaselineAdjuster(double Percent) : IOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOffboardingBaselineAdjuster(double Minimum) : IOffboardingBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the offboarding baseline steps in order and keeps a trace of each.</summary>
public sealed class OffboardingBaselineEngine(List<IOffboardingBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingBaselineEngine DefaultOffboardingBaselineEngine()
    {
        return new OffboardingBaselineEngine(
            new List<IOffboardingBaselineAdjuster>
            {
                new RoundingOffboardingBaselineAdjuster(0.5),
                new TitleOffboardingBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.33, ["online"] = 1.1, ["east"] = 1.24 }),
                new PercentOffboardingBaselineAdjuster(5.2),
                new MinimumOffboardingBaselineAdjuster(45.1),
            },
            new List<string>(),
            4.7,
            90260.2);
    }

    public double Run(OffboardingBaseline item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
