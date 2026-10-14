namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan exception calculation.</summary>
public interface ISuccessionPlanExceptionAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanException item, double valueValue);
}

public sealed record FlaggedSuccessionPlanExceptionAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSuccessionPlanExceptionAdjuster(double Minimum) : ISuccessionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSuccessionPlanExceptionAdjuster(double Step) : ISuccessionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamSuccessionPlanExceptionAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanExceptionAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SuccessionPlanException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSuccessionPlanExceptionAdjuster(double Percent) : ISuccessionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the succession plan exception steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanExceptionEngine(List<ISuccessionPlanExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanExceptionEngine DefaultSuccessionPlanExceptionEngine(Registry flags)
    {
        return new SuccessionPlanExceptionEngine(
            new List<ISuccessionPlanExceptionAdjuster>
            {
                new FlaggedSuccessionPlanExceptionAdjuster(flags, "succession-plan-exception-capped", -5.1),
                new MinimumSuccessionPlanExceptionAdjuster(17.3),
                new RoundingSuccessionPlanExceptionAdjuster(0.5),
                new TeamSuccessionPlanExceptionAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.72, ["east"] = 1.11, ["central"] = 0.75 }),
                new PercentSuccessionPlanExceptionAdjuster(-5.9),
            },
            new List<string>(),
            2.5,
            5283.6);
    }

    public double Price(SuccessionPlanException item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
