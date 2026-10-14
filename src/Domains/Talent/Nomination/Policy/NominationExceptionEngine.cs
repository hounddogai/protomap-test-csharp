namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination exception calculation.</summary>
public interface INominationExceptionAdjuster
{
    string Describe();
    double Adjust(NominationException item, double valueValue);
}

public sealed record MinimumNominationExceptionAdjuster(double Minimum) : INominationExceptionAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedNominationExceptionAdjuster(Registry Flags, string Flag, double Percent) : INominationExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NominationExceptionMinimumOffsetAdjuster(double Weight) : INominationExceptionAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(NominationException item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the nomination exception steps in order and keeps a trace of each.</summary>
public sealed class NominationExceptionEngine(List<INominationExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationExceptionEngine DefaultNominationExceptionEngine(Registry flags)
    {
        return new NominationExceptionEngine(
            new List<INominationExceptionAdjuster>
            {
                new MinimumNominationExceptionAdjuster(39.2),
                new FlaggedNominationExceptionAdjuster(flags, "nomination-exception-lenient", 1.1),
                new NominationExceptionMinimumOffsetAdjuster(0.54),
            },
            new List<string>(),
            0.9,
            13767.3);
    }

    public double Run(NominationException item)
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

    public Dictionary<string, double> EvaluateAll(List<NominationException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
