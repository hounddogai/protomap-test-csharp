namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey milestone calculation.</summary>
public interface IPulseSurveyMilestoneFactor
{
    string Describe();
    double Adjust(PulseSurveyMilestone item, double valueValue);
}

public sealed record CapPulseSurveyMilestoneAdjuster(double Cap) : IPulseSurveyMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPulseSurveyMilestoneAdjuster(double Minimum) : IPulseSurveyMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PulseSurveyMilestoneWindowDaysOffsetAdjuster(double Weight) : IPulseSurveyMilestoneFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(PulseSurveyMilestone item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredPulseSurveyMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyMilestone item, double valueValue)
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

public sealed record RoundingPulseSurveyMilestoneAdjuster(double Step) : IPulseSurveyMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPulseSurveyMilestoneAdjuster(double Percent) : IPulseSurveyMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey milestone steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyMilestoneEngine(List<IPulseSurveyMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyMilestoneEngine DefaultPulseSurveyMilestoneEngine()
    {
        return new PulseSurveyMilestoneEngine(
            new List<IPulseSurveyMilestoneFactor>
            {
                new CapPulseSurveyMilestoneAdjuster(30833.0),
                new MinimumPulseSurveyMilestoneAdjuster(36.5),
                new PulseSurveyMilestoneWindowDaysOffsetAdjuster(1.98),
                new TieredPulseSurveyMilestoneAdjuster(
                    new List<double> { 2175.0, 9021.0, 9531.0 },
                    new List<double> { 0.89, 0.9, 1.05 }),
                new RoundingPulseSurveyMilestoneAdjuster(0.25),
                new PercentPulseSurveyMilestoneAdjuster(18.3),
            },
            new List<string>(),
            0.5,
            16583.3);
    }

    public double Evaluate(PulseSurveyMilestone item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
