namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding request calculation.</summary>
public interface IOffboardingRequestModifier
{
    string Describe();
    double Adjust(OffboardingRequest item, double valueValue);
}

public sealed record CapOffboardingRequestAdjuster(double Cap) : IOffboardingRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumOffboardingRequestAdjuster(double Minimum) : IOffboardingRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingOffboardingRequestAdjuster(double Step) : IOffboardingRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record OffboardingRequestDurationMinutesOffsetAdjuster(double Weight) : IOffboardingRequestModifier
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(OffboardingRequest item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record FlaggedOffboardingRequestAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentOffboardingRequestAdjuster(double Percent) : IOffboardingRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding request steps in order and keeps a trace of each.</summary>
public sealed class OffboardingRequestEngine(List<IOffboardingRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingRequestEngine DefaultOffboardingRequestEngine(Registry flags)
    {
        return new OffboardingRequestEngine(
            new List<IOffboardingRequestModifier>
            {
                new CapOffboardingRequestAdjuster(3047.0),
                new MinimumOffboardingRequestAdjuster(41.4),
                new RoundingOffboardingRequestAdjuster(5.0),
                new OffboardingRequestDurationMinutesOffsetAdjuster(1.08),
                new FlaggedOffboardingRequestAdjuster(flags, "offboarding-request-seasonal", 0.7),
                new PercentOffboardingRequestAdjuster(-20.1),
            },
            new List<string>(),
            3.9,
            65439.1);
    }

    public double Compute(OffboardingRequest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
