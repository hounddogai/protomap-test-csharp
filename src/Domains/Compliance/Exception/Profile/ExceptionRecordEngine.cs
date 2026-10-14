namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception record calculation.</summary>
public interface IExceptionRecordModifier
{
    string Describe();
    double Adjust(ExceptionRecord item, double valueValue);
}

public sealed record RoundingExceptionRecordAdjuster(double Step) : IExceptionRecordModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumExceptionRecordAdjuster(double Minimum) : IExceptionRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleExceptionRecordAdjuster(Dictionary<string, double> Factors) : IExceptionRecordModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ExceptionRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentExceptionRecordAdjuster(double Percent) : IExceptionRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception record steps in order and keeps a trace of each.</summary>
public sealed class ExceptionRecordEngine(List<IExceptionRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionRecordEngine DefaultExceptionRecordEngine()
    {
        return new ExceptionRecordEngine(
            new List<IExceptionRecordModifier>
            {
                new RoundingExceptionRecordAdjuster(0.1),
                new MinimumExceptionRecordAdjuster(5.7),
                new TitleExceptionRecordAdjuster(new Dictionary<string, double> { ["retail"] = 1.35 }),
                new PercentExceptionRecordAdjuster(-20.5),
            },
            new List<string>(),
            0.9,
            33523.3);
    }

    public double Price(ExceptionRecord item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
