namespace Acme.Workforce.Payrollops.Garnishment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment mapping must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentMappingRule
{
    List<string> Check(GarnishmentMapping item);
}

public sealed record GarnishmentMappingVersionRangeRule(int Min, int Max) : IGarnishmentMappingRule
{
    public List<string> Check(GarnishmentMapping item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMappingCodeLengthRule(int MaxLength) : IGarnishmentMappingRule
{
    public List<string> Check(GarnishmentMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMappingRequiredNameRule(string Message) : IGarnishmentMappingRule
{
    public List<string> Check(GarnishmentMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMappingCodePrefixRule(string Prefix) : IGarnishmentMappingRule
{
    public List<string> Check(GarnishmentMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMappingAllowedNameRule(List<string> Allowed) : IGarnishmentMappingRule
{
    public List<string> Check(GarnishmentMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment mapping.</summary>
public sealed record GarnishmentMappingValidator(List<IGarnishmentMappingRule> Rules)
{
    public List<string> Validate(GarnishmentMapping item)
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

    public void RequireValid(GarnishmentMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentMapping> items)
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

/// <summary>Functions of garnishment mapping rules.</summary>
public static class GarnishmentMappingRules
{
    public static GarnishmentMappingValidator DefaultGarnishmentMappingValidator()
    {
        return new GarnishmentMappingValidator(
            new List<IGarnishmentMappingRule>
            {
                new GarnishmentMappingVersionRangeRule(255, 1000),
                new GarnishmentMappingCodeLengthRule(3600),
                new GarnishmentMappingRequiredNameRule("garnishment mapping needs an entry"),
                new GarnishmentMappingCodePrefixRule("X"),
                new GarnishmentMappingAllowedNameRule(new List<string>()),
            });
    }
}
