namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling recognition slot calculation.</summary>
public interface IRollingRecognitionSlotModifier
{
    string Describe();
    double Adjust(RollingRecognitionSlot item, double valueValue);
}

public sealed record RoundingRollingRecognitionSlotAdjuster(double Step) : IRollingRecognitionSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingRecognitionSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryRollingRecognitionSlotAdjuster(Dictionary<string, double> Factors) : IRollingRecognitionSlotModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RollingRecognitionSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRollingRecognitionSlotAdjuster(double Cap) : IRollingRecognitionSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingRecognitionSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling recognition slot steps in order and keeps a trace of each.</summary>
public sealed class RollingRecognitionSlotEngine(List<IRollingRecognitionSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRecognitionSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRecognitionSlotEngine DefaultRollingRecognitionSlotEngine()
    {
        return new RollingRecognitionSlotEngine(
            new List<IRollingRecognitionSlotModifier>
            {
                new RoundingRollingRecognitionSlotAdjuster(0.25),
                new CountryRollingRecognitionSlotAdjuster(new Dictionary<string, double> { ["retail"] = 0.84 }),
                new CapRollingRecognitionSlotAdjuster(42100.0),
            },
            new List<string>(),
            3.4,
            20284.7);
    }

    public double Run(RollingRecognitionSlot item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingRecognitionSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
