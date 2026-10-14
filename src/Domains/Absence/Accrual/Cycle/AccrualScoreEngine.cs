namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual score calculation.</summary>
public interface IAccrualScoreAdjuster
{
    string Describe();
    double Adjust(AccrualScore item, double valueValue);
}

public sealed record CapAccrualScoreAdjuster(double Cap) : IAccrualScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAccrualScoreAdjuster(List<double> Bounds, List<double> Rates) : IAccrualScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualScore item, double valueValue)
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

public sealed record FlaggedAccrualScoreAdjuster(Registry Flags, string Flag, double Percent) : IAccrualScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccrualScoreAdjuster(double Minimum) : IAccrualScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAccrualScoreAdjuster(double Step) : IAccrualScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceAccrualScoreAdjuster(Dictionary<string, double> Factors) : IAccrualScoreAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AccrualScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the accrual score steps in order and keeps a trace of each.</summary>
public sealed class AccrualScoreEngine(List<IAccrualScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualScoreEngine DefaultAccrualScoreEngine(Registry flags)
    {
        return new AccrualScoreEngine(
            new List<IAccrualScoreAdjuster>
            {
                new CapAccrualScoreAdjuster(36304.0),
                new TieredAccrualScoreAdjuster(new List<double> { 1350.0, 5186.0 }, new List<double> { 0.97, 0.86 }),
                new FlaggedAccrualScoreAdjuster(flags, "accrual-score-lenient", -1.6),
                new MinimumAccrualScoreAdjuster(13.6),
                new RoundingAccrualScoreAdjuster(0.25),
                new SourceAccrualScoreAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.17, ["east"] = 0.92, ["online"] = 1.02 }),
            },
            new List<string>(),
            3.2,
            59724.1);
    }

    public double Price(AccrualScore item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
