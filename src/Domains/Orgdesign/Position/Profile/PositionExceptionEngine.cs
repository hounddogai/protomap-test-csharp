namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position exception calculation.</summary>
public interface IPositionExceptionFactor
{
    string Describe();
    double Adjust(PositionException item, double valueValue);
}

public sealed record PercentPositionExceptionAdjuster(double Percent) : IPositionExceptionFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPositionExceptionAdjuster(double Cap) : IPositionExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPositionExceptionAdjuster(double Minimum) : IPositionExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPositionExceptionAdjuster(double Step) : IPositionExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the position exception steps in order and keeps a trace of each.</summary>
public sealed class PositionExceptionEngine(List<IPositionExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionExceptionEngine DefaultPositionExceptionEngine()
    {
        return new PositionExceptionEngine(
            new List<IPositionExceptionFactor>
            {
                new PercentPositionExceptionAdjuster(19.7),
                new CapPositionExceptionAdjuster(34388.0),
                new MinimumPositionExceptionAdjuster(0.5),
                new RoundingPositionExceptionAdjuster(5.0),
            },
            new List<string>(),
            0.6,
            87577.4);
    }

    public double Evaluate(PositionException item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
