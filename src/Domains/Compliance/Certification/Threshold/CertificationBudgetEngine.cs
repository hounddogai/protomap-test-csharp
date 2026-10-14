namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification budget calculation.</summary>
public interface ICertificationBudgetAdjuster
{
    string Describe();
    double Adjust(CertificationBudget item, double valueValue);
}

public sealed record RoundingCertificationBudgetAdjuster(double Step) : ICertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCertificationBudgetAdjuster(double Cap) : ICertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCertificationBudgetAdjuster(Registry Flags, string Flag, double Percent) : ICertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCertificationBudgetAdjuster(double Percent) : ICertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the certification budget steps in order and keeps a trace of each.</summary>
public sealed class CertificationBudgetEngine(List<ICertificationBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationBudgetEngine DefaultCertificationBudgetEngine(Registry flags)
    {
        return new CertificationBudgetEngine(
            new List<ICertificationBudgetAdjuster>
            {
                new RoundingCertificationBudgetAdjuster(0.5),
                new CapCertificationBudgetAdjuster(8273.0),
                new FlaggedCertificationBudgetAdjuster(flags, "certification-budget-regional", -1.1),
                new PercentCertificationBudgetAdjuster(-22.0),
            },
            new List<string>(),
            3.2,
            57208.7);
    }

    public double Price(CertificationBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<CertificationBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
