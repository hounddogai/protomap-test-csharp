namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path claim calculation.</summary>
public interface ICareerPathClaimModifier
{
    string Describe();
    double Adjust(CareerPathClaim item, double valueValue);
}

public sealed record UnitCareerPathClaimAdjuster(Dictionary<string, double> Factors) : ICareerPathClaimModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(CareerPathClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCareerPathClaimAdjuster(double Percent) : ICareerPathClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCareerPathClaimAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCareerPathClaimAdjuster(double Step) : ICareerPathClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCareerPathClaimAdjuster(double Cap) : ICareerPathClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the career path claim steps in order and keeps a trace of each.</summary>
public sealed class CareerPathClaimEngine(List<ICareerPathClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathClaimEngine DefaultCareerPathClaimEngine(Registry flags)
    {
        return new CareerPathClaimEngine(
            new List<ICareerPathClaimModifier>
            {
                new UnitCareerPathClaimAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.99, ["west"] = 0.76, ["retail"] = 0.93 }),
                new PercentCareerPathClaimAdjuster(1.0),
                new FlaggedCareerPathClaimAdjuster(flags, "career-path-claim-primary", -0.4),
                new RoundingCareerPathClaimAdjuster(0.05),
                new CapCareerPathClaimAdjuster(36575.0),
            },
            new List<string>(),
            2.2,
            27501.8);
    }

    public double Compute(CareerPathClaim item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
