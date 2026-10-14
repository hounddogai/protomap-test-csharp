namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path window calculation.</summary>
public interface ICareerPathWindowFactor
{
    string Describe();
    double Adjust(CareerPathWindow item, double valueValue);
}

public sealed record CareerPathWindowPriorityOffsetAdjuster(double Weight) : ICareerPathWindowFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(CareerPathWindow item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record FlaggedCareerPathWindowAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathWindowFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCareerPathWindowAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathWindowFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathWindow item, double valueValue)
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

public sealed record RoundingCareerPathWindowAdjuster(double Step) : ICareerPathWindowFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamCareerPathWindowAdjuster(Dictionary<string, double> Factors) : ICareerPathWindowFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(CareerPathWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCareerPathWindowAdjuster(double Minimum) : ICareerPathWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the career path window steps in order and keeps a trace of each.</summary>
public sealed class CareerPathWindowEngine(List<ICareerPathWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathWindowEngine DefaultCareerPathWindowEngine(Registry flags)
    {
        return new CareerPathWindowEngine(
            new List<ICareerPathWindowFactor>
            {
                new CareerPathWindowPriorityOffsetAdjuster(0.86),
                new FlaggedCareerPathWindowAdjuster(flags, "career-path-window-lenient", -2.1),
                new TieredCareerPathWindowAdjuster(
                    new List<double> { 4581.0, 7269.0 },
                    new List<double> { 0.86, 1.15 }),
                new RoundingCareerPathWindowAdjuster(5.0),
                new TeamCareerPathWindowAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.23, ["south"] = 0.8 }),
                new MinimumCareerPathWindowAdjuster(17.6),
            },
            new List<string>(),
            3.4,
            8862.9);
    }

    public double Evaluate(CareerPathWindow item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
