namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal pulse survey quota calculation.</summary>
public interface ISeasonalPulseSurveyQuotaFactor
{
    string Describe();
    double Adjust(SeasonalPulseSurveyQuota item, double valueValue);
}

public sealed record RoundingSeasonalPulseSurveyQuotaAdjuster(double Step) : ISeasonalPulseSurveyQuotaFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalPulseSurveyQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSeasonalPulseSurveyQuotaAdjuster(double Percent) : ISeasonalPulseSurveyQuotaFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPulseSurveyQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalPulseSurveyQuotaMaximumOffsetAdjuster(double Weight) : ISeasonalPulseSurveyQuotaFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(SeasonalPulseSurveyQuota item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record CapSeasonalPulseSurveyQuotaAdjuster(double Cap) : ISeasonalPulseSurveyQuotaFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalPulseSurveyQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalPulseSurveyQuotaAdjuster(double Minimum) : ISeasonalPulseSurveyQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPulseSurveyQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSeasonalPulseSurveyQuotaAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalPulseSurveyQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalPulseSurveyQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal pulse survey quota steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPulseSurveyQuotaEngine(List<ISeasonalPulseSurveyQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPulseSurveyQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPulseSurveyQuotaEngine DefaultSeasonalPulseSurveyQuotaEngine(Registry flags)
    {
        return new SeasonalPulseSurveyQuotaEngine(
            new List<ISeasonalPulseSurveyQuotaFactor>
            {
                new RoundingSeasonalPulseSurveyQuotaAdjuster(0.1),
                new PercentSeasonalPulseSurveyQuotaAdjuster(-0.1),
                new SeasonalPulseSurveyQuotaMaximumOffsetAdjuster(0.58),
                new CapSeasonalPulseSurveyQuotaAdjuster(33178.0),
                new MinimumSeasonalPulseSurveyQuotaAdjuster(28.2),
                new FlaggedSeasonalPulseSurveyQuotaAdjuster(flags, "seasonal-pulse-survey-quota-capped", -1.1),
            },
            new List<string>(),
            0.8,
            81258.8);
    }

    public double Evaluate(SeasonalPulseSurveyQuota item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPulseSurveyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
