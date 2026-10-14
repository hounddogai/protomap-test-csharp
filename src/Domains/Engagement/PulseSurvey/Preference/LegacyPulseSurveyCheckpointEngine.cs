namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pulse survey checkpoint calculation.</summary>
public interface ILegacyPulseSurveyCheckpointFactor
{
    string Describe();
    double Adjust(LegacyPulseSurveyCheckpoint item, double valueValue);
}

public sealed record MinimumLegacyPulseSurveyCheckpointAdjuster(double Minimum) : ILegacyPulseSurveyCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPulseSurveyCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LegacyPulseSurveyCheckpointMinimumOffsetAdjuster(double Weight) : ILegacyPulseSurveyCheckpointFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(LegacyPulseSurveyCheckpoint item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record PercentLegacyPulseSurveyCheckpointAdjuster(double Percent) : ILegacyPulseSurveyCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPulseSurveyCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyPulseSurveyCheckpointAdjuster(double Cap) : ILegacyPulseSurveyCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPulseSurveyCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy pulse survey checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LegacyPulseSurveyCheckpointEngine(List<ILegacyPulseSurveyCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPulseSurveyCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPulseSurveyCheckpointEngine DefaultLegacyPulseSurveyCheckpointEngine()
    {
        return new LegacyPulseSurveyCheckpointEngine(
            new List<ILegacyPulseSurveyCheckpointFactor>
            {
                new MinimumLegacyPulseSurveyCheckpointAdjuster(16.7),
                new LegacyPulseSurveyCheckpointMinimumOffsetAdjuster(0.46),
                new PercentLegacyPulseSurveyCheckpointAdjuster(-3.3),
                new CapLegacyPulseSurveyCheckpointAdjuster(14211.0),
            },
            new List<string>(),
            0.8,
            26659.1);
    }

    public double Evaluate(LegacyPulseSurveyCheckpoint item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPulseSurveyCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
