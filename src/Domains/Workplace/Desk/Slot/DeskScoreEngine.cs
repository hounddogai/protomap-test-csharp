namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the desk score calculation.</summary>
public interface IDeskScoreAdjuster
{
    string Describe();
    double Adjust(DeskScore item, double valueValue);
}

public sealed record DeskScoreBatchSizeOffsetAdjuster(double Weight) : IDeskScoreAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(DeskScore item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapDeskScoreAdjuster(double Cap) : IDeskScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDeskScoreAdjuster(double Percent) : IDeskScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeskScoreAdjuster(double Minimum) : IDeskScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDeskScoreAdjuster(Registry Flags, string Flag, double Percent) : IDeskScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskScoreAdjuster(List<double> Bounds, List<double> Rates) : IDeskScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskScore item, double valueValue)
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

/// <summary>Runs the desk score steps in order and keeps a trace of each.</summary>
public sealed class DeskScoreEngine(List<IDeskScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskScoreEngine DefaultDeskScoreEngine(Registry flags)
    {
        return new DeskScoreEngine(
            new List<IDeskScoreAdjuster>
            {
                new DeskScoreBatchSizeOffsetAdjuster(1.79),
                new CapDeskScoreAdjuster(14368.0),
                new PercentDeskScoreAdjuster(-24.5),
                new MinimumDeskScoreAdjuster(20.3),
                new FlaggedDeskScoreAdjuster(flags, "desk-score-capped", 4.4),
                new TieredDeskScoreAdjuster(
                    new List<double> { 1072.0, 6830.0, 8141.0, 9386.0 },
                    new List<double> { 1.16, 0.92, 0.89, 1.08 }),
            },
            new List<string>(),
            0.9,
            84721.3);
    }

    public double Evaluate(DeskScore item)
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

    public Dictionary<string, double> EvaluateAll(List<DeskScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
