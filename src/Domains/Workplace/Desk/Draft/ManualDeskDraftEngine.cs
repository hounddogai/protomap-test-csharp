namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual desk draft calculation.</summary>
public interface IManualDeskDraftFactor
{
    string Describe();
    double Adjust(ManualDeskDraft item, double valueValue);
}

public sealed record TieredManualDeskDraftAdjuster(List<double> Bounds, List<double> Rates) : IManualDeskDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualDeskDraft item, double valueValue)
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

public sealed record MinimumManualDeskDraftAdjuster(double Minimum) : IManualDeskDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualDeskDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentManualDeskDraftAdjuster(double Percent) : IManualDeskDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualDeskDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelManualDeskDraftAdjuster(Dictionary<string, double> Factors) : IManualDeskDraftFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ManualDeskDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapManualDeskDraftAdjuster(double Cap) : IManualDeskDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualDeskDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual desk draft steps in order and keeps a trace of each.</summary>
public sealed class ManualDeskDraftEngine(List<IManualDeskDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualDeskDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualDeskDraftEngine DefaultManualDeskDraftEngine()
    {
        return new ManualDeskDraftEngine(
            new List<IManualDeskDraftFactor>
            {
                new TieredManualDeskDraftAdjuster(
                    new List<double> { 2839.0, 3243.0 },
                    new List<double> { 1.15, 1.18 }),
                new MinimumManualDeskDraftAdjuster(9.1),
                new PercentManualDeskDraftAdjuster(-19.4),
                new LabelManualDeskDraftAdjuster(new Dictionary<string, double> { ["online"] = 1.08 }),
                new CapManualDeskDraftAdjuster(36839.0),
            },
            new List<string>(),
            1.3,
            16139.0);
    }

    public double Evaluate(ManualDeskDraft item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualDeskDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
