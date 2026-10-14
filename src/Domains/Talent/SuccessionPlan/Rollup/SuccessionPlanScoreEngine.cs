namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan score calculation.</summary>
public interface ISuccessionPlanScoreFactor
{
    string Describe();
    double Adjust(SuccessionPlanScore item, double valueValue);
}

public sealed record TitleSuccessionPlanScoreAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanScoreFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SuccessionPlanScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSuccessionPlanScoreAdjuster(double Step) : ISuccessionPlanScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSuccessionPlanScoreAdjuster(double Percent) : ISuccessionPlanScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the succession plan score steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanScoreEngine(List<ISuccessionPlanScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanScoreEngine DefaultSuccessionPlanScoreEngine()
    {
        return new SuccessionPlanScoreEngine(
            new List<ISuccessionPlanScoreFactor>
            {
                new TitleSuccessionPlanScoreAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.35, ["north"] = 1.11 }),
                new RoundingSuccessionPlanScoreAdjuster(5.0),
                new PercentSuccessionPlanScoreAdjuster(-23.0),
            },
            new List<string>(),
            4.0,
            33646.5);
    }

    public double Compute(SuccessionPlanScore item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
