namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg schedule calculation.</summary>
public interface IReorgScheduleAdjuster
{
    string Describe();
    double Adjust(ReorgSchedule item, double valueValue);
}

public sealed record PercentReorgScheduleAdjuster(double Percent) : IReorgScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReorgScheduleAdjuster(List<double> Bounds, List<double> Rates) : IReorgScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgSchedule item, double valueValue)
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

public sealed record RoundingReorgScheduleAdjuster(double Step) : IReorgScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the reorg schedule steps in order and keeps a trace of each.</summary>
public sealed class ReorgScheduleEngine(List<IReorgScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgScheduleEngine DefaultReorgScheduleEngine()
    {
        return new ReorgScheduleEngine(
            new List<IReorgScheduleAdjuster>
            {
                new PercentReorgScheduleAdjuster(9.9),
                new TieredReorgScheduleAdjuster(
                    new List<double> { 6489.0, 7348.0, 7708.0, 9681.0 },
                    new List<double> { 1.03, 0.89, 0.97, 0.84 }),
                new RoundingReorgScheduleAdjuster(1.0),
            },
            new List<string>(),
            1.2,
            77276.3);
    }

    public double Evaluate(ReorgSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
