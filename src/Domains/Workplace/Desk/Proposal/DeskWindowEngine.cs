namespace Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>One step of the desk window calculation.</summary>
public interface IDeskWindowAdjuster
{
    string Describe();
    double Adjust(DeskWindow item, double valueValue);
}

public sealed record PercentDeskWindowAdjuster(double Percent) : IDeskWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeskWindowAdjuster(double Cap) : IDeskWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDeskWindowAdjuster(double Minimum) : IDeskWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the desk window steps in order and keeps a trace of each.</summary>
public sealed class DeskWindowEngine(List<IDeskWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskWindowEngine DefaultDeskWindowEngine()
    {
        return new DeskWindowEngine(
            new List<IDeskWindowAdjuster>
            {
                new PercentDeskWindowAdjuster(20.3),
                new CapDeskWindowAdjuster(35637.0),
                new MinimumDeskWindowAdjuster(49.3),
            },
            new List<string>(),
            4.5,
            71215.8);
    }

    public double Price(DeskWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<DeskWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
