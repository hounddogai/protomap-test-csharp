namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination score calculation.</summary>
public interface INominationScoreStep
{
    string Describe();
    double Adjust(NominationScore item, double valueValue);
}

public sealed record TieredNominationScoreAdjuster(List<double> Bounds, List<double> Rates) : INominationScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationScore item, double valueValue)
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

public sealed record CapNominationScoreAdjuster(double Cap) : INominationScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingNominationScoreAdjuster(double Step) : INominationScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitNominationScoreAdjuster(Dictionary<string, double> Factors) : INominationScoreStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(NominationScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumNominationScoreAdjuster(double Minimum) : INominationScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NominationScoreEndsAtOffsetAdjuster(double Weight) : INominationScoreStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(NominationScore item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the nomination score steps in order and keeps a trace of each.</summary>
public sealed class NominationScoreEngine(List<INominationScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationScoreEngine DefaultNominationScoreEngine()
    {
        return new NominationScoreEngine(
            new List<INominationScoreStep>
            {
                new TieredNominationScoreAdjuster(
                    new List<double> { 1340.0, 6387.0, 8293.0 },
                    new List<double> { 1.1, 1.01, 0.83 }),
                new CapNominationScoreAdjuster(39781.0),
                new RoundingNominationScoreAdjuster(5.0),
                new UnitNominationScoreAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.72, ["south"] = 0.71 }),
                new MinimumNominationScoreAdjuster(30.9),
                new NominationScoreEndsAtOffsetAdjuster(1.28),
            },
            new List<string>(),
            0.3,
            54663.4);
    }

    public double Compute(NominationScore item)
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

    public Dictionary<string, double> EvaluateAll(List<NominationScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
