namespace Acme.Workforce.Contractors.Vendor.Score;

/// <summary>One step of the adaptive vendor assignment calculation.</summary>
public interface IAdaptiveVendorAssignmentModifier
{
    string Describe();
    double Adjust(AdaptiveVendorAssignment item, double valueValue);
}

public sealed record PercentAdaptiveVendorAssignmentAdjuster(double Percent) : IAdaptiveVendorAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveVendorAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveVendorAssignmentLimitOffsetAdjuster(double Weight) : IAdaptiveVendorAssignmentModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AdaptiveVendorAssignment item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CapAdaptiveVendorAssignmentAdjuster(double Cap) : IAdaptiveVendorAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveVendorAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive vendor assignment steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveVendorAssignmentEngine(List<IAdaptiveVendorAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveVendorAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveVendorAssignmentEngine DefaultAdaptiveVendorAssignmentEngine()
    {
        return new AdaptiveVendorAssignmentEngine(
            new List<IAdaptiveVendorAssignmentModifier>
            {
                new PercentAdaptiveVendorAssignmentAdjuster(22.0),
                new AdaptiveVendorAssignmentLimitOffsetAdjuster(0.01),
                new CapAdaptiveVendorAssignmentAdjuster(44640.0),
            },
            new List<string>(),
            3.8,
            9273.6);
    }

    public double Price(AdaptiveVendorAssignment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveVendorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
