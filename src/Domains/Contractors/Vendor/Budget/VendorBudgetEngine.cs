namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor budget calculation.</summary>
public interface IVendorBudgetFactor
{
    string Describe();
    double Adjust(VendorBudget item, double valueValue);
}

public sealed record MinimumVendorBudgetAdjuster(double Minimum) : IVendorBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapVendorBudgetAdjuster(double Cap) : IVendorBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentVendorBudgetAdjuster(double Percent) : IVendorBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VendorBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVendorBudgetAdjuster(double Step) : IVendorBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the vendor budget steps in order and keeps a trace of each.</summary>
public sealed class VendorBudgetEngine(List<IVendorBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorBudgetEngine DefaultVendorBudgetEngine()
    {
        return new VendorBudgetEngine(
            new List<IVendorBudgetFactor>
            {
                new MinimumVendorBudgetAdjuster(39.1),
                new CapVendorBudgetAdjuster(8993.0),
                new PercentVendorBudgetAdjuster(-18.3),
                new RoundingVendorBudgetAdjuster(0.25),
            },
            new List<string>(),
            1.5,
            77117.7);
    }

    public double Evaluate(VendorBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<VendorBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
