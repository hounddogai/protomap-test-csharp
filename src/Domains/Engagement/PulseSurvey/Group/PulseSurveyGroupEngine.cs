namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey group calculation.</summary>
public interface IPulseSurveyGroupModifier
{
    string Describe();
    double Adjust(PulseSurveyGroup item, double valueValue);
}

public sealed record CategoryPulseSurveyGroupAdjuster(Dictionary<string, double> Factors) : IPulseSurveyGroupModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PulseSurveyGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPulseSurveyGroupAdjuster(double Cap) : IPulseSurveyGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPulseSurveyGroupAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPulseSurveyGroupAdjuster(double Step) : IPulseSurveyGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pulse survey group steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyGroupEngine(List<IPulseSurveyGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyGroupEngine DefaultPulseSurveyGroupEngine(Registry flags)
    {
        return new PulseSurveyGroupEngine(
            new List<IPulseSurveyGroupModifier>
            {
                new CategoryPulseSurveyGroupAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.29, ["online"] = 0.72 }),
                new CapPulseSurveyGroupAdjuster(36519.0),
                new FlaggedPulseSurveyGroupAdjuster(flags, "pulse-survey-group-seasonal", -1.8),
                new RoundingPulseSurveyGroupAdjuster(0.1),
            },
            new List<string>(),
            0.5,
            20700.2);
    }

    public double Run(PulseSurveyGroup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
