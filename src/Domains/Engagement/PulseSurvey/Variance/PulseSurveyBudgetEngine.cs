namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey budget calculation.</summary>
public interface IPulseSurveyBudgetFactor
{
    string Describe();
    double Adjust(PulseSurveyBudget item, double valueValue);
}

public sealed record FlaggedPulseSurveyBudgetAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyBudgetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPulseSurveyBudgetAdjuster(double Cap) : IPulseSurveyBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelPulseSurveyBudgetAdjuster(Dictionary<string, double> Factors) : IPulseSurveyBudgetFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PulseSurveyBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PulseSurveyBudgetRatioOffsetAdjuster(double Weight) : IPulseSurveyBudgetFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(PulseSurveyBudget item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record PercentPulseSurveyBudgetAdjuster(double Percent) : IPulseSurveyBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey budget steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyBudgetEngine(List<IPulseSurveyBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyBudgetEngine DefaultPulseSurveyBudgetEngine(Registry flags)
    {
        return new PulseSurveyBudgetEngine(
            new List<IPulseSurveyBudgetFactor>
            {
                new FlaggedPulseSurveyBudgetAdjuster(flags, "pulse-survey-budget-scheduled", -7.2),
                new CapPulseSurveyBudgetAdjuster(21325.0),
                new LabelPulseSurveyBudgetAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.32, ["online"] = 0.73, ["south"] = 0.88 }),
                new PulseSurveyBudgetRatioOffsetAdjuster(1.91),
                new PercentPulseSurveyBudgetAdjuster(-5.7),
            },
            new List<string>(),
            3.9,
            15610.9);
    }

    public double Evaluate(PulseSurveyBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
