namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped merit cycle score calculation.</summary>
public interface ICappedMeritCycleScoreAdjuster
{
    string Describe();
    double Adjust(CappedMeritCycleScore item, double valueValue);
}

public sealed record FlaggedCappedMeritCycleScoreAdjuster(Registry Flags, string Flag, double Percent) : ICappedMeritCycleScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedMeritCycleScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedMeritCycleScoreAdjuster(double Step) : ICappedMeritCycleScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedMeritCycleScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedMeritCycleScoreAdjuster(List<double> Bounds, List<double> Rates) : ICappedMeritCycleScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedMeritCycleScore item, double valueValue)
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

/// <summary>Runs the capped merit cycle score steps in order and keeps a trace of each.</summary>
public sealed class CappedMeritCycleScoreEngine(List<ICappedMeritCycleScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedMeritCycleScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedMeritCycleScoreEngine DefaultCappedMeritCycleScoreEngine(Registry flags)
    {
        return new CappedMeritCycleScoreEngine(
            new List<ICappedMeritCycleScoreAdjuster>
            {
                new FlaggedCappedMeritCycleScoreAdjuster(flags, "capped-merit-cycle-score-scheduled", -0.2),
                new RoundingCappedMeritCycleScoreAdjuster(0.05),
                new TieredCappedMeritCycleScoreAdjuster(
                    new List<double> { 1459.0, 8057.0, 9132.0 },
                    new List<double> { 1.18, 1.01, 1.08 }),
            },
            new List<string>(),
            1.5,
            93671.2);
    }

    public double Compute(CappedMeritCycleScore item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedMeritCycleScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
