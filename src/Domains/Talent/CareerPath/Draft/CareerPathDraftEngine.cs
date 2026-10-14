namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the career path draft calculation.</summary>
public interface ICareerPathDraftStep
{
    string Describe();
    double Adjust(CareerPathDraft item, double valueValue);
}

public sealed record CapCareerPathDraftAdjuster(double Cap) : ICareerPathDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCareerPathDraftAdjuster(double Minimum) : ICareerPathDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCareerPathDraftAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CareerPathDraftVersionOffsetAdjuster(double Weight) : ICareerPathDraftStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(CareerPathDraft item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentCareerPathDraftAdjuster(double Percent) : ICareerPathDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path draft steps in order and keeps a trace of each.</summary>
public sealed class CareerPathDraftEngine(List<ICareerPathDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathDraftEngine DefaultCareerPathDraftEngine(Registry flags)
    {
        return new CareerPathDraftEngine(
            new List<ICareerPathDraftStep>
            {
                new CapCareerPathDraftAdjuster(20280.0),
                new MinimumCareerPathDraftAdjuster(9.3),
                new FlaggedCareerPathDraftAdjuster(flags, "career-path-draft-capped", 3.9),
                new CareerPathDraftVersionOffsetAdjuster(1.88),
                new PercentCareerPathDraftAdjuster(-0.4),
            },
            new List<string>(),
            3.9,
            77223.9);
    }

    public double Price(CareerPathDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
