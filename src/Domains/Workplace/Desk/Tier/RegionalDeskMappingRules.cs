namespace Acme.Workforce.Workplace.Desk.Tier;

/// <summary>A rule a regional desk mapping must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeskMappingRule
{
    List<string> Check(RegionalDeskMapping item);
}

public sealed record RegionalDeskMappingOwnerIdLengthRule(int MaxLength) : IRegionalDeskMappingRule
{
    public List<string> Check(RegionalDeskMapping item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskMappingRequiredCountryRule(string Message) : IRegionalDeskMappingRule
{
    public List<string> Check(RegionalDeskMapping item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskMappingCodeLengthRule(int MaxLength) : IRegionalDeskMappingRule
{
    public List<string> Check(RegionalDeskMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskMappingRequiredOwnerIdRule(string Message) : IRegionalDeskMappingRule
{
    public List<string> Check(RegionalDeskMapping item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional desk mapping.</summary>
public sealed record RegionalDeskMappingValidator(List<IRegionalDeskMappingRule> Rules)
{
    public List<string> Validate(RegionalDeskMapping item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(RegionalDeskMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeskMapping> items)
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

/// <summary>Functions of regional desk mapping rules.</summary>
public static class RegionalDeskMappingRules
{
    public static RegionalDeskMappingValidator DefaultRegionalDeskMappingValidator()
    {
        return new RegionalDeskMappingValidator(
            new List<IRegionalDeskMappingRule>
            {
                new RegionalDeskMappingOwnerIdLengthRule(3600),
                new RegionalDeskMappingRequiredCountryRule("regional desk mapping needs an entry"),
                new RegionalDeskMappingCodeLengthRule(255),
                new RegionalDeskMappingRequiredOwnerIdRule("regional desk mapping needs an entry"),
            });
    }
}
