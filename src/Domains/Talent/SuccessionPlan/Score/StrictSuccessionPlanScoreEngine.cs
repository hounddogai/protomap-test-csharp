namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict succession plan score calculation.</summary>
public interface IStrictSuccessionPlanScoreModifier
{
    string Describe();
    double Adjust(StrictSuccessionPlanScore item, double valueValue);
}

public sealed record TieredStrictSuccessionPlanScoreAdjuster(List<double> Bounds, List<double> Rates) : IStrictSuccessionPlanScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictSuccessionPlanScore item, double valueValue)
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

public sealed record RoundingStrictSuccessionPlanScoreAdjuster(double Step) : IStrictSuccessionPlanScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictSuccessionPlanScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedStrictSuccessionPlanScoreAdjuster(Registry Flags, string Flag, double Percent) : IStrictSuccessionPlanScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictSuccessionPlanScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict succession plan score steps in order and keeps a trace of each.</summary>
public sealed class StrictSuccessionPlanScoreEngine(List<IStrictSuccessionPlanScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSuccessionPlanScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSuccessionPlanScoreEngine DefaultStrictSuccessionPlanScoreEngine(Registry flags)
    {
        return new StrictSuccessionPlanScoreEngine(
            new List<IStrictSuccessionPlanScoreModifier>
            {
                new TieredStrictSuccessionPlanScoreAdjuster(
                    new List<double> { 3724.0, 5574.0, 9490.0 },
                    new List<double> { 1.17, 1.03, 0.97 }),
                new RoundingStrictSuccessionPlanScoreAdjuster(0.5),
                new FlaggedStrictSuccessionPlanScoreAdjuster(flags, "strict-succession-plan-score-strict", 7.0),
            },
            new List<string>(),
            2.2,
            59113.7);
    }

    public double Price(StrictSuccessionPlanScore item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictSuccessionPlanScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
