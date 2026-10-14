namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>One step of the default pay run request calculation.</summary>
public interface IDefaultPayRunRequestModifier
{
    string Describe();
    double Adjust(DefaultPayRunRequest item, double valueValue);
}

public sealed record DefaultPayRunRequestFteOffsetAdjuster(double Weight) : IDefaultPayRunRequestModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(DefaultPayRunRequest item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumDefaultPayRunRequestAdjuster(double Minimum) : IDefaultPayRunRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayRunRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDefaultPayRunRequestAdjuster(double Cap) : IDefaultPayRunRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayRunRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDefaultPayRunRequestAdjuster(double Percent) : IDefaultPayRunRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayRunRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default pay run request steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayRunRequestEngine(List<IDefaultPayRunRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayRunRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayRunRequestEngine DefaultDefaultPayRunRequestEngine()
    {
        return new DefaultPayRunRequestEngine(
            new List<IDefaultPayRunRequestModifier>
            {
                new DefaultPayRunRequestFteOffsetAdjuster(0.56),
                new MinimumDefaultPayRunRequestAdjuster(12.8),
                new CapDefaultPayRunRequestAdjuster(12604.0),
                new PercentDefaultPayRunRequestAdjuster(21.7),
            },
            new List<string>(),
            2.6,
            81535.8);
    }

    public double Price(DefaultPayRunRequest item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayRunRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
