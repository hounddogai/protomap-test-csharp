namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path segment calculation.</summary>
public interface ICareerPathSegmentModifier
{
    string Describe();
    double Adjust(CareerPathSegment item, double valueValue);
}

public sealed record FlaggedCareerPathSegmentAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathSegmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCareerPathSegmentAdjuster(double Step) : ICareerPathSegmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCareerPathSegmentAdjuster(double Minimum) : ICareerPathSegmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the career path segment steps in order and keeps a trace of each.</summary>
public sealed class CareerPathSegmentEngine(List<ICareerPathSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathSegmentEngine DefaultCareerPathSegmentEngine(Registry flags)
    {
        return new CareerPathSegmentEngine(
            new List<ICareerPathSegmentModifier>
            {
                new FlaggedCareerPathSegmentAdjuster(flags, "career-path-segment-legacy", 2.9),
                new RoundingCareerPathSegmentAdjuster(0.25),
                new MinimumCareerPathSegmentAdjuster(22.0),
            },
            new List<string>(),
            4.5,
            14733.1);
    }

    public double Compute(CareerPathSegment item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
