namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition allocation calculation.</summary>
public interface IRecognitionAllocationFactor
{
    string Describe();
    double Adjust(RecognitionAllocation item, double valueValue);
}

public sealed record MinimumRecognitionAllocationAdjuster(double Minimum) : IRecognitionAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelRecognitionAllocationAdjuster(Dictionary<string, double> Factors) : IRecognitionAllocationFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RecognitionAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRecognitionAllocationAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition allocation steps in order and keeps a trace of each.</summary>
public sealed class RecognitionAllocationEngine(List<IRecognitionAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionAllocationEngine DefaultRecognitionAllocationEngine(Registry flags)
    {
        return new RecognitionAllocationEngine(
            new List<IRecognitionAllocationFactor>
            {
                new MinimumRecognitionAllocationAdjuster(3.7),
                new LabelRecognitionAllocationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["north"] = 1.24, ["online"] = 1.3 }),
                new FlaggedRecognitionAllocationAdjuster(flags, "recognition-allocation-global", -9.5),
            },
            new List<string>(),
            0.5,
            42641.3);
    }

    public double Price(RecognitionAllocation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
