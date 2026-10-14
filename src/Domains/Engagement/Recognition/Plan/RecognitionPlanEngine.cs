namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the recognition plan calculation.</summary>
public interface IRecognitionPlanFactor
{
    string Describe();
    double Adjust(RecognitionPlan item, double valueValue);
}

public sealed record FlaggedRecognitionPlanAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionPlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRecognitionPlanAdjuster(double Cap) : IRecognitionPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRecognitionPlanAdjuster(double Percent) : IRecognitionPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition plan steps in order and keeps a trace of each.</summary>
public sealed class RecognitionPlanEngine(List<IRecognitionPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionPlanEngine DefaultRecognitionPlanEngine(Registry flags)
    {
        return new RecognitionPlanEngine(
            new List<IRecognitionPlanFactor>
            {
                new FlaggedRecognitionPlanAdjuster(flags, "recognition-plan-lenient", 4.6),
                new CapRecognitionPlanAdjuster(25810.0),
                new PercentRecognitionPlanAdjuster(24.5),
            },
            new List<string>(),
            4.2,
            51642.5);
    }

    public double Evaluate(RecognitionPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
