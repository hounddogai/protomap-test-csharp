namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall score calculation.</summary>
public interface ITownHallScoreFactor
{
    string Describe();
    double Adjust(TownHallScore item, double valueValue);
}

public sealed record TownHallScoreAmountOffsetAdjuster(double Weight) : ITownHallScoreFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(TownHallScore item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentTownHallScoreAdjuster(double Percent) : ITownHallScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTownHallScoreAdjuster(double Step) : ITownHallScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTownHallScoreAdjuster(double Minimum) : ITownHallScoreFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the town hall score steps in order and keeps a trace of each.</summary>
public sealed class TownHallScoreEngine(List<ITownHallScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallScoreEngine DefaultTownHallScoreEngine()
    {
        return new TownHallScoreEngine(
            new List<ITownHallScoreFactor>
            {
                new TownHallScoreAmountOffsetAdjuster(0.6),
                new PercentTownHallScoreAdjuster(-13.9),
                new RoundingTownHallScoreAdjuster(0.05),
                new MinimumTownHallScoreAdjuster(24.8),
            },
            new List<string>(),
            1.2,
            62823.1);
    }

    public double Compute(TownHallScore item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<TownHallScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
