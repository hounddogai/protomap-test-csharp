namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default recognition threshold calculation.</summary>
public interface IDefaultRecognitionThresholdModifier
{
    string Describe();
    double Adjust(DefaultRecognitionThreshold item, double valueValue);
}

public sealed record CapDefaultRecognitionThresholdAdjuster(double Cap) : IDefaultRecognitionThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultRecognitionThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultRecognitionThresholdFloorValueOffsetAdjuster(double Weight) : IDefaultRecognitionThresholdModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(DefaultRecognitionThreshold item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record RoundingDefaultRecognitionThresholdAdjuster(double Step) : IDefaultRecognitionThresholdModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultRecognitionThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultRecognitionThresholdAdjuster(double Minimum) : IDefaultRecognitionThresholdModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultRecognitionThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default recognition threshold steps in order and keeps a trace of each.</summary>
public sealed class DefaultRecognitionThresholdEngine(List<IDefaultRecognitionThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultRecognitionThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultRecognitionThresholdEngine DefaultDefaultRecognitionThresholdEngine()
    {
        return new DefaultRecognitionThresholdEngine(
            new List<IDefaultRecognitionThresholdModifier>
            {
                new CapDefaultRecognitionThresholdAdjuster(22976.0),
                new DefaultRecognitionThresholdFloorValueOffsetAdjuster(1.89),
                new RoundingDefaultRecognitionThresholdAdjuster(0.25),
                new MinimumDefaultRecognitionThresholdAdjuster(25.2),
            },
            new List<string>(),
            5.0,
            74719.4);
    }

    public double Evaluate(DefaultRecognitionThreshold item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultRecognitionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
