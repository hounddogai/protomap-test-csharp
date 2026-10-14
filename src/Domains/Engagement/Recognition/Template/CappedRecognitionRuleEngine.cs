namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped recognition rule calculation.</summary>
public interface ICappedRecognitionRuleFactor
{
    string Describe();
    double Adjust(CappedRecognitionRule item, double valueValue);
}

public sealed record CapCappedRecognitionRuleAdjuster(double Cap) : ICappedRecognitionRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedRecognitionRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedRecognitionRuleAdjuster(Registry Flags, string Flag, double Percent) : ICappedRecognitionRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedRecognitionRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedRecognitionRuleAdjuster(double Step) : ICappedRecognitionRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRecognitionRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped recognition rule steps in order and keeps a trace of each.</summary>
public sealed class CappedRecognitionRuleEngine(List<ICappedRecognitionRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRecognitionRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRecognitionRuleEngine DefaultCappedRecognitionRuleEngine(Registry flags)
    {
        return new CappedRecognitionRuleEngine(
            new List<ICappedRecognitionRuleFactor>
            {
                new CapCappedRecognitionRuleAdjuster(45323.0),
                new FlaggedCappedRecognitionRuleAdjuster(flags, "capped-recognition-rule-primary", 8.3),
                new RoundingCappedRecognitionRuleAdjuster(0.1),
            },
            new List<string>(),
            0.8,
            79178.0);
    }

    public double Compute(CappedRecognitionRule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRecognitionRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
