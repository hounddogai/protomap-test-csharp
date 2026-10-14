namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending skill profile forecast calculation.</summary>
public interface IPendingSkillProfileForecastStep
{
    string Describe();
    double Adjust(PendingSkillProfileForecast item, double valueValue);
}

public sealed record FlaggedPendingSkillProfileForecastAdjuster(Registry Flags, string Flag, double Percent) : IPendingSkillProfileForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingSkillProfileForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPendingSkillProfileForecastAdjuster(double Percent) : IPendingSkillProfileForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingSkillProfileForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingSkillProfileForecastAdjuster(double Cap) : IPendingSkillProfileForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingSkillProfileForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPendingSkillProfileForecastAdjuster(double Step) : IPendingSkillProfileForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingSkillProfileForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending skill profile forecast steps in order and keeps a trace of each.</summary>
public sealed class PendingSkillProfileForecastEngine(List<IPendingSkillProfileForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSkillProfileForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSkillProfileForecastEngine DefaultPendingSkillProfileForecastEngine(Registry flags)
    {
        return new PendingSkillProfileForecastEngine(
            new List<IPendingSkillProfileForecastStep>
            {
                new FlaggedPendingSkillProfileForecastAdjuster(
                    flags,
                    "pending-skill-profile-forecast-seasonal",
                    -9.1),
                new PercentPendingSkillProfileForecastAdjuster(-20.3),
                new CapPendingSkillProfileForecastAdjuster(24147.0),
                new RoundingPendingSkillProfileForecastAdjuster(0.25),
            },
            new List<string>(),
            4.9,
            1530.8);
    }

    public double Run(PendingSkillProfileForecast item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingSkillProfileForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
