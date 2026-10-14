namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall profile calculation.</summary>
public interface ITownHallProfileFactor
{
    string Describe();
    double Adjust(TownHallProfile item, double valueValue);
}

public sealed record DepartmentTownHallProfileAdjuster(Dictionary<string, double> Factors) : ITownHallProfileFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(TownHallProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTownHallProfileAdjuster(double Step) : ITownHallProfileFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTownHallProfileAdjuster(double Percent) : ITownHallProfileFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall profile steps in order and keeps a trace of each.</summary>
public sealed class TownHallProfileEngine(List<ITownHallProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallProfileEngine DefaultTownHallProfileEngine()
    {
        return new TownHallProfileEngine(
            new List<ITownHallProfileFactor>
            {
                new DepartmentTownHallProfileAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.79, ["south"] = 0.92, ["central"] = 0.86 }),
                new RoundingTownHallProfileAdjuster(0.5),
                new PercentTownHallProfileAdjuster(-11.0),
            },
            new List<string>(),
            1.1,
            13604.7);
    }

    public double Evaluate(TownHallProfile item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
