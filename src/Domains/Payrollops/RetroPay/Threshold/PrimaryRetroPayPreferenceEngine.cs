namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary retro pay preference calculation.</summary>
public interface IPrimaryRetroPayPreferenceStep
{
    string Describe();
    double Adjust(PrimaryRetroPayPreference item, double valueValue);
}

public sealed record MinimumPrimaryRetroPayPreferenceAdjuster(double Minimum) : IPrimaryRetroPayPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryRetroPayPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryRetroPayPreferenceCapacityOffsetAdjuster(double Weight) : IPrimaryRetroPayPreferenceStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PrimaryRetroPayPreference item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedPrimaryRetroPayPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryRetroPayPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryRetroPayPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryRetroPayPreferenceAdjuster(double Percent) : IPrimaryRetroPayPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryRetroPayPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryRetroPayPreferenceAdjuster(double Step) : IPrimaryRetroPayPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryRetroPayPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary retro pay preference steps in order and keeps a trace of each.</summary>
public sealed class PrimaryRetroPayPreferenceEngine(List<IPrimaryRetroPayPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryRetroPayPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryRetroPayPreferenceEngine DefaultPrimaryRetroPayPreferenceEngine(Registry flags)
    {
        return new PrimaryRetroPayPreferenceEngine(
            new List<IPrimaryRetroPayPreferenceStep>
            {
                new MinimumPrimaryRetroPayPreferenceAdjuster(12.3),
                new PrimaryRetroPayPreferenceCapacityOffsetAdjuster(1.7),
                new FlaggedPrimaryRetroPayPreferenceAdjuster(flags, "primary-retro-pay-preference-global", -7.6),
                new PercentPrimaryRetroPayPreferenceAdjuster(13.2),
                new RoundingPrimaryRetroPayPreferenceAdjuster(1.0),
            },
            new List<string>(),
            3.1,
            37131.4);
    }

    public double Compute(PrimaryRetroPayPreference item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryRetroPayPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
