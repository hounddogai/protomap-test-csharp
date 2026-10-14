namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception template calculation.</summary>
public interface IExceptionTemplateStep
{
    string Describe();
    double Adjust(ExceptionTemplate item, double valueValue);
}

public sealed record ExceptionTemplateWindowDaysOffsetAdjuster(double Weight) : IExceptionTemplateStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ExceptionTemplate item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record FlaggedExceptionTemplateAdjuster(Registry Flags, string Flag, double Percent) : IExceptionTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumExceptionTemplateAdjuster(double Minimum) : IExceptionTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredExceptionTemplateAdjuster(List<double> Bounds, List<double> Rates) : IExceptionTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionTemplate item, double valueValue)
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

/// <summary>Runs the exception template steps in order and keeps a trace of each.</summary>
public sealed class ExceptionTemplateEngine(List<IExceptionTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionTemplateEngine DefaultExceptionTemplateEngine(Registry flags)
    {
        return new ExceptionTemplateEngine(
            new List<IExceptionTemplateStep>
            {
                new ExceptionTemplateWindowDaysOffsetAdjuster(1.52),
                new FlaggedExceptionTemplateAdjuster(flags, "exception-template-pending", 5.5),
                new MinimumExceptionTemplateAdjuster(45.1),
                new TieredExceptionTemplateAdjuster(
                    new List<double> { 5969.0, 9670.0 },
                    new List<double> { 1.13, 1.15 }),
            },
            new List<string>(),
            1.2,
            40765.5);
    }

    public double Price(ExceptionTemplate item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
