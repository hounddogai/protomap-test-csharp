namespace Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>A rule a occupancy override must pass. Check returns its problems, or none.</summary>
public interface IOccupancyOverrideRule
{
    List<string> Check(OccupancyOverride item);
}

public sealed record OccupancyOverrideCodeLengthRule(int MaxLength) : IOccupancyOverrideRule
{
    public List<string> Check(OccupancyOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyOverrideCodePrefixRule(string Prefix) : IOccupancyOverrideRule
{
    public List<string> Check(OccupancyOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyOverrideRequiredCodeRule(string Message) : IOccupancyOverrideRule
{
    public List<string> Check(OccupancyOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy override.</summary>
public sealed record OccupancyOverrideValidator(List<IOccupancyOverrideRule> Rules)
{
    public List<string> Validate(OccupancyOverride item)
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

    public void RequireValid(OccupancyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyOverride> items)
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

/// <summary>Functions of occupancy override rules.</summary>
public static class OccupancyOverrideRules
{
    public static OccupancyOverrideValidator DefaultOccupancyOverrideValidator()
    {
        return new OccupancyOverrideValidator(
            new List<IOccupancyOverrideRule>
            {
                new OccupancyOverrideCodeLengthRule(16),
                new OccupancyOverrideCodePrefixRule("OPS"),
                new OccupancyOverrideRequiredCodeRule("occupancy override needs this field"),
            });
    }
}
