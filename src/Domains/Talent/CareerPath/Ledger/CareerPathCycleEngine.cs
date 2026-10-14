namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path cycle calculation.</summary>
public interface ICareerPathCycleStep
{
    string Describe();
    double Adjust(CareerPathCycle item, double valueValue);
}

public sealed record RoundingCareerPathCycleAdjuster(double Step) : ICareerPathCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCareerPathCycleAdjuster(double Cap) : ICareerPathCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CareerPathCycleMaximumOffsetAdjuster(double Weight) : ICareerPathCycleStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(CareerPathCycle item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record DepartmentCareerPathCycleAdjuster(Dictionary<string, double> Factors) : ICareerPathCycleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(CareerPathCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCareerPathCycleAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path cycle steps in order and keeps a trace of each.</summary>
public sealed class CareerPathCycleEngine(List<ICareerPathCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathCycleEngine DefaultCareerPathCycleEngine(Registry flags)
    {
        return new CareerPathCycleEngine(
            new List<ICareerPathCycleStep>
            {
                new RoundingCareerPathCycleAdjuster(1.0),
                new CapCareerPathCycleAdjuster(22639.0),
                new CareerPathCycleMaximumOffsetAdjuster(0.75),
                new DepartmentCareerPathCycleAdjuster(new Dictionary<string, double> { ["east"] = 0.77 }),
                new FlaggedCareerPathCycleAdjuster(flags, "career-path-cycle-capped", 3.1),
            },
            new List<string>(),
            2.8,
            37803.0);
    }

    public double Price(CareerPathCycle item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
