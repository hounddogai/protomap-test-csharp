namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run mapping calculation.</summary>
public interface IPayRunMappingFactor
{
    string Describe();
    double Adjust(PayRunMapping item, double valueValue);
}

public sealed record MinimumPayRunMappingAdjuster(double Minimum) : IPayRunMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPayRunMappingAdjuster(double Cap) : IPayRunMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPayRunMappingAdjuster(double Percent) : IPayRunMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePayRunMappingAdjuster(Dictionary<string, double> Factors) : IPayRunMappingFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PayRunMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay run mapping steps in order and keeps a trace of each.</summary>
public sealed class PayRunMappingEngine(List<IPayRunMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunMappingEngine DefaultPayRunMappingEngine()
    {
        return new PayRunMappingEngine(
            new List<IPayRunMappingFactor>
            {
                new MinimumPayRunMappingAdjuster(8.3),
                new CapPayRunMappingAdjuster(12808.0),
                new PercentPayRunMappingAdjuster(24.0),
                new NamePayRunMappingAdjuster(new Dictionary<string, double> { ["south"] = 1.1, ["west"] = 0.79 }),
            },
            new List<string>(),
            2.3,
            15146.1);
    }

    public double Run(PayRunMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
