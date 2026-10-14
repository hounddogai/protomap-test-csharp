namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled retro pay score calculation.</summary>
public interface IScheduledRetroPayScoreStep
{
    string Describe();
    double Adjust(ScheduledRetroPayScore item, double valueValue);
}

public sealed record ScheduledRetroPayScoreGraceDaysOffsetAdjuster(double Weight) : IScheduledRetroPayScoreStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ScheduledRetroPayScore item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record RoundingScheduledRetroPayScoreAdjuster(double Step) : IScheduledRetroPayScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledRetroPayScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitScheduledRetroPayScoreAdjuster(Dictionary<string, double> Factors) : IScheduledRetroPayScoreStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ScheduledRetroPayScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledRetroPayScoreAdjuster(double Cap) : IScheduledRetroPayScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledRetroPayScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScheduledRetroPayScoreAdjuster(List<double> Bounds, List<double> Rates) : IScheduledRetroPayScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledRetroPayScore item, double valueValue)
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

public sealed record MinimumScheduledRetroPayScoreAdjuster(double Minimum) : IScheduledRetroPayScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledRetroPayScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled retro pay score steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRetroPayScoreEngine(List<IScheduledRetroPayScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRetroPayScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRetroPayScoreEngine DefaultScheduledRetroPayScoreEngine()
    {
        return new ScheduledRetroPayScoreEngine(
            new List<IScheduledRetroPayScoreStep>
            {
                new ScheduledRetroPayScoreGraceDaysOffsetAdjuster(0.35),
                new RoundingScheduledRetroPayScoreAdjuster(0.1),
                new UnitScheduledRetroPayScoreAdjuster(new Dictionary<string, double> { ["south"] = 1.16 }),
                new CapScheduledRetroPayScoreAdjuster(46802.0),
                new TieredScheduledRetroPayScoreAdjuster(
                    new List<double> { 459.0, 7780.0 },
                    new List<double> { 0.81, 0.99 }),
                new MinimumScheduledRetroPayScoreAdjuster(32.7),
            },
            new List<string>(),
            3.3,
            14560.6);
    }

    public double Evaluate(ScheduledRetroPayScore item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRetroPayScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
