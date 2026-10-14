namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination mapping must pass. Check returns its problems, or none.</summary>
public interface INominationMappingRule
{
    List<string> Check(NominationMapping item);
}

public sealed record NominationMappingRequiredPhoneRule(string Message) : INominationMappingRule
{
    public List<string> Check(NominationMapping item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationMappingRequiredNameRule(string Message) : INominationMappingRule
{
    public List<string> Check(NominationMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationMappingCodePrefixRule(string Prefix) : INominationMappingRule
{
    public List<string> Check(NominationMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record NominationMappingCodeLengthRule(int MaxLength) : INominationMappingRule
{
    public List<string> Check(NominationMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationMappingNameLengthRule(int MaxLength) : INominationMappingRule
{
    public List<string> Check(NominationMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationMappingAllowedNameRule(List<string> Allowed) : INominationMappingRule
{
    public List<string> Check(NominationMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination mapping.</summary>
public sealed record NominationMappingValidator(List<INominationMappingRule> Rules)
{
    public List<string> Validate(NominationMapping item)
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

    public void RequireValid(NominationMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationMapping> items)
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

/// <summary>Functions of nomination mapping rules.</summary>
public static class NominationMappingRules
{
    public static NominationMappingValidator DefaultNominationMappingValidator()
    {
        return new NominationMappingValidator(
            new List<INominationMappingRule>
            {
                new NominationMappingRequiredPhoneRule("nomination mapping needs this field"),
                new NominationMappingRequiredNameRule("nomination mapping needs an entry"),
                new NominationMappingCodePrefixRule("ACM"),
                new NominationMappingCodeLengthRule(86400),
                new NominationMappingNameLengthRule(86400),
                new NominationMappingAllowedNameRule(new List<string> { "global", "manual" }),
            });
    }
}
