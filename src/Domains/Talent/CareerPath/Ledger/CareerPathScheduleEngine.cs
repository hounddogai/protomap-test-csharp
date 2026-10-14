namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path schedule calculation.</summary>
public interface ICareerPathScheduleStep
{
    string Describe();
    double Adjust(CareerPathSchedule item, double valueValue);
}

public sealed record SegmentCareerPathScheduleAdjuster(Dictionary<string, double> Factors) : ICareerPathScheduleStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CareerPathSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CareerPathScheduleDurationMinutesOffsetAdjuster(double Weight) : ICareerPathScheduleStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(CareerPathSchedule item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record RoundingCareerPathScheduleAdjuster(double Step) : ICareerPathScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the career path schedule steps in order and keeps a trace of each.</summary>
public sealed class CareerPathScheduleEngine(List<ICareerPathScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathScheduleEngine DefaultCareerPathScheduleEngine()
    {
        return new CareerPathScheduleEngine(
            new List<ICareerPathScheduleStep>
            {
                new SegmentCareerPathScheduleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.72, ["retail"] = 0.73 }),
                new CareerPathScheduleDurationMinutesOffsetAdjuster(2.0),
                new RoundingCareerPathScheduleAdjuster(0.5),
            },
            new List<string>(),
            2.9,
            74541.8);
    }

    public double Price(CareerPathSchedule item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
