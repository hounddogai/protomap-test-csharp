namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary merit cycle rollup calculation.</summary>
public interface IPrimaryMeritCycleRollupFactor
{
    string Describe();
    double Adjust(PrimaryMeritCycleRollup item, double valueValue);
}

public sealed record TitlePrimaryMeritCycleRollupAdjuster(Dictionary<string, double> Factors) : IPrimaryMeritCycleRollupFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryMeritCycleRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPrimaryMeritCycleRollupAdjuster(double Step) : IPrimaryMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryMeritCycleRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryMeritCycleRollupAdjuster(double Percent) : IPrimaryMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryMeritCycleRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryMeritCycleRollupAdjuster(double Cap) : IPrimaryMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryMeritCycleRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryMeritCycleRollupAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryMeritCycleRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryMeritCycleRollupGraceDaysOffsetAdjuster(double Weight) : IPrimaryMeritCycleRollupFactor
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(PrimaryMeritCycleRollup item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the primary merit cycle rollup steps in order and keeps a trace of each.</summary>
public sealed class PrimaryMeritCycleRollupEngine(List<IPrimaryMeritCycleRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryMeritCycleRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryMeritCycleRollupEngine DefaultPrimaryMeritCycleRollupEngine(Registry flags)
    {
        return new PrimaryMeritCycleRollupEngine(
            new List<IPrimaryMeritCycleRollupFactor>
            {
                new TitlePrimaryMeritCycleRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.24, ["south"] = 0.87 }),
                new RoundingPrimaryMeritCycleRollupAdjuster(5.0),
                new PercentPrimaryMeritCycleRollupAdjuster(13.1),
                new CapPrimaryMeritCycleRollupAdjuster(36614.0),
                new FlaggedPrimaryMeritCycleRollupAdjuster(flags, "primary-merit-cycle-rollup-strict", -0.2),
                new PrimaryMeritCycleRollupGraceDaysOffsetAdjuster(1.17),
            },
            new List<string>(),
            2.8,
            83786.7);
    }

    public double Price(PrimaryMeritCycleRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryMeritCycleRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
