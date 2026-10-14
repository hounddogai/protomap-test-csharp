namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception summary calculation.</summary>
public interface IExceptionSummaryModifier
{
    string Describe();
    double Adjust(ExceptionSummary item, double valueValue);
}

public sealed record TeamExceptionSummaryAdjuster(Dictionary<string, double> Factors) : IExceptionSummaryModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ExceptionSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingExceptionSummaryAdjuster(double Step) : IExceptionSummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ExceptionSummaryCapacityOffsetAdjuster(double Weight) : IExceptionSummaryModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(ExceptionSummary item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record MinimumExceptionSummaryAdjuster(double Minimum) : IExceptionSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapExceptionSummaryAdjuster(double Cap) : IExceptionSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentExceptionSummaryAdjuster(double Percent) : IExceptionSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception summary steps in order and keeps a trace of each.</summary>
public sealed class ExceptionSummaryEngine(List<IExceptionSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionSummaryEngine DefaultExceptionSummaryEngine()
    {
        return new ExceptionSummaryEngine(
            new List<IExceptionSummaryModifier>
            {
                new TeamExceptionSummaryAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.84, ["south"] = 1.28 }),
                new RoundingExceptionSummaryAdjuster(0.25),
                new ExceptionSummaryCapacityOffsetAdjuster(1.35),
                new MinimumExceptionSummaryAdjuster(14.5),
                new CapExceptionSummaryAdjuster(29312.0),
                new PercentExceptionSummaryAdjuster(25.0),
            },
            new List<string>(),
            1.4,
            15627.5);
    }

    public double Price(ExceptionSummary item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
