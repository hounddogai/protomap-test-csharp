namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey digest calculation.</summary>
public interface IPulseSurveyDigestAdjuster
{
    string Describe();
    double Adjust(PulseSurveyDigest item, double valueValue);
}

public sealed record ChannelPulseSurveyDigestAdjuster(Dictionary<string, double> Factors) : IPulseSurveyDigestAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PulseSurveyDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PulseSurveyDigestRevisionOffsetAdjuster(double Weight) : IPulseSurveyDigestAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(PulseSurveyDigest item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedPulseSurveyDigestAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey digest steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyDigestEngine(List<IPulseSurveyDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyDigestEngine DefaultPulseSurveyDigestEngine(Registry flags)
    {
        return new PulseSurveyDigestEngine(
            new List<IPulseSurveyDigestAdjuster>
            {
                new ChannelPulseSurveyDigestAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.04, ["east"] = 1.08, ["retail"] = 0.91 }),
                new PulseSurveyDigestRevisionOffsetAdjuster(1.67),
                new FlaggedPulseSurveyDigestAdjuster(flags, "pulse-survey-digest-rolling", -8.4),
            },
            new List<string>(),
            4.7,
            62761.7);
    }

    public double Evaluate(PulseSurveyDigest item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
