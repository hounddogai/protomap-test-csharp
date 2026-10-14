namespace Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>A rule a mobility mapping must pass. Check returns its problems, or none.</summary>
public interface IMobilityMappingRule
{
    List<string> Check(MobilityMapping item);
}

public sealed record MobilityMappingCodePrefixRule(string Prefix) : IMobilityMappingRule
{
    public List<string> Check(MobilityMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityMappingLocationLengthRule(int MaxLength) : IMobilityMappingRule
{
    public List<string> Check(MobilityMapping item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityMappingRequiredLocationRule(string Message) : IMobilityMappingRule
{
    public List<string> Check(MobilityMapping item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility mapping.</summary>
public sealed record MobilityMappingValidator(List<IMobilityMappingRule> Rules)
{
    public List<string> Validate(MobilityMapping item)
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

    public void RequireValid(MobilityMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityMapping> items)
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

/// <summary>Functions of mobility mapping rules.</summary>
public static class MobilityMappingRules
{
    public static MobilityMappingValidator DefaultMobilityMappingValidator()
    {
        return new MobilityMappingValidator(
            new List<IMobilityMappingRule>
            {
                new MobilityMappingCodePrefixRule("Q"),
                new MobilityMappingLocationLengthRule(32),
                new MobilityMappingRequiredLocationRule("mobility mapping needs this field"),
            });
    }
}
