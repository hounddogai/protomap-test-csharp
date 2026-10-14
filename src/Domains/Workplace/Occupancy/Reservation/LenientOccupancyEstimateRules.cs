namespace Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>A rule a lenient occupancy estimate must pass. Check returns its problems, or none.</summary>
public interface ILenientOccupancyEstimateRule
{
    List<string> Check(LenientOccupancyEstimate item);
}

public sealed record LenientOccupancyEstimateRequiredManagerIdRule(string Message) : ILenientOccupancyEstimateRule
{
    public List<string> Check(LenientOccupancyEstimate item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientOccupancyEstimateCodePrefixRule(string Prefix) : ILenientOccupancyEstimateRule
{
    public List<string> Check(LenientOccupancyEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOccupancyEstimateLabelLengthRule(int MaxLength) : ILenientOccupancyEstimateRule
{
    public List<string> Check(LenientOccupancyEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientOccupancyEstimateRequiredNameRule(string Message) : ILenientOccupancyEstimateRule
{
    public List<string> Check(LenientOccupancyEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientOccupancyEstimateRequiredLabelRule(string Message) : ILenientOccupancyEstimateRule
{
    public List<string> Check(LenientOccupancyEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient occupancy estimate.</summary>
public sealed record LenientOccupancyEstimateValidator(List<ILenientOccupancyEstimateRule> Rules)
{
    public List<string> Validate(LenientOccupancyEstimate item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(LenientOccupancyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientOccupancyEstimate> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of lenient occupancy estimate rules.</summary>
public static class LenientOccupancyEstimateRules
{
    public static LenientOccupancyEstimateValidator DefaultLenientOccupancyEstimateValidator()
    {
        return new LenientOccupancyEstimateValidator(
            new List<ILenientOccupancyEstimateRule>
            {
                new LenientOccupancyEstimateRequiredManagerIdRule("lenient occupancy estimate needs a value"),
                new LenientOccupancyEstimateCodePrefixRule("OPS"),
                new LenientOccupancyEstimateLabelLengthRule(86400),
                new LenientOccupancyEstimateRequiredNameRule("lenient occupancy estimate needs a value"),
                new LenientOccupancyEstimateRequiredLabelRule("lenient occupancy estimate needs this field"),
            });
    }
}
