namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path group calculation.</summary>
public interface ICareerPathGroupFactor
{
    string Describe();
    double Adjust(CareerPathGroup item, double valueValue);
}

public sealed record TieredCareerPathGroupAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathGroupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathGroup item, double valueValue)
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

public sealed record MinimumCareerPathGroupAdjuster(double Minimum) : ICareerPathGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCareerPathGroupAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathGroupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCareerPathGroupAdjuster(double Step) : ICareerPathGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the career path group steps in order and keeps a trace of each.</summary>
public sealed class CareerPathGroupEngine(List<ICareerPathGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathGroupEngine DefaultCareerPathGroupEngine(Registry flags)
    {
        return new CareerPathGroupEngine(
            new List<ICareerPathGroupFactor>
            {
                new TieredCareerPathGroupAdjuster(
                    new List<double> { 1545.0, 3634.0, 4002.0, 7098.0 },
                    new List<double> { 0.88, 1.04, 0.84, 0.82 }),
                new MinimumCareerPathGroupAdjuster(25.9),
                new FlaggedCareerPathGroupAdjuster(flags, "career-path-group-manual", 4.3),
                new RoundingCareerPathGroupAdjuster(0.1),
            },
            new List<string>(),
            3.1,
            68092.1);
    }

    public double Run(CareerPathGroup item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
