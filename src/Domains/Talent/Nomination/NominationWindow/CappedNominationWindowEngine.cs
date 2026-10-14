namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped nomination window calculation.</summary>
public interface ICappedNominationWindowModifier
{
    string Describe();
    double Adjust(CappedNominationWindow item, double valueValue);
}

public sealed record CapCappedNominationWindowAdjuster(double Cap) : ICappedNominationWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedNominationWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCappedNominationWindowAdjuster(double Minimum) : ICappedNominationWindowModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedNominationWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedNominationWindowAdjuster(List<double> Bounds, List<double> Rates) : ICappedNominationWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedNominationWindow item, double valueValue)
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

public sealed record RoundingCappedNominationWindowAdjuster(double Step) : ICappedNominationWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedNominationWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped nomination window steps in order and keeps a trace of each.</summary>
public sealed class CappedNominationWindowEngine(List<ICappedNominationWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedNominationWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedNominationWindowEngine DefaultCappedNominationWindowEngine()
    {
        return new CappedNominationWindowEngine(
            new List<ICappedNominationWindowModifier>
            {
                new CapCappedNominationWindowAdjuster(20007.0),
                new MinimumCappedNominationWindowAdjuster(49.4),
                new TieredCappedNominationWindowAdjuster(
                    new List<double> { 415.0, 5198.0, 9393.0, 9906.0 },
                    new List<double> { 1.12, 0.87, 1.18, 0.93 }),
                new RoundingCappedNominationWindowAdjuster(0.25),
            },
            new List<string>(),
            0.4,
            50631.4);
    }

    public double Price(CappedNominationWindow item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedNominationWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
