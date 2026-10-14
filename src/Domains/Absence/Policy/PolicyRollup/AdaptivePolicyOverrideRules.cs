namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive policy override must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePolicyOverrideRule
{
    List<string> Check(AdaptivePolicyOverride item);
}

public sealed record AdaptivePolicyOverrideCodeLengthRule(int MaxLength) : IAdaptivePolicyOverrideRule
{
    public List<string> Check(AdaptivePolicyOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyOverrideChecksumLengthRule(int MaxLength) : IAdaptivePolicyOverrideRule
{
    public List<string> Check(AdaptivePolicyOverride item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyOverrideNameLengthRule(int MaxLength) : IAdaptivePolicyOverrideRule
{
    public List<string> Check(AdaptivePolicyOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyOverrideRequiredNameRule(string Message) : IAdaptivePolicyOverrideRule
{
    public List<string> Check(AdaptivePolicyOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyOverrideRequiredCodeRule(string Message) : IAdaptivePolicyOverrideRule
{
    public List<string> Check(AdaptivePolicyOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyOverrideAllowedChecksumRule(List<string> Allowed) : IAdaptivePolicyOverrideRule
{
    public List<string> Check(AdaptivePolicyOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive policy override.</summary>
public sealed record AdaptivePolicyOverrideValidator(List<IAdaptivePolicyOverrideRule> Rules)
{
    public List<string> Validate(AdaptivePolicyOverride item)
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

    public void RequireValid(AdaptivePolicyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePolicyOverride> items)
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

/// <summary>Functions of adaptive policy override rules.</summary>
public static class AdaptivePolicyOverrideRules
{
    public static AdaptivePolicyOverrideValidator DefaultAdaptivePolicyOverrideValidator()
    {
        return new AdaptivePolicyOverrideValidator(
            new List<IAdaptivePolicyOverrideRule>
            {
                new AdaptivePolicyOverrideCodeLengthRule(255),
                new AdaptivePolicyOverrideChecksumLengthRule(80),
                new AdaptivePolicyOverrideNameLengthRule(1000),
                new AdaptivePolicyOverrideRequiredNameRule("adaptive policy override needs this field"),
                new AdaptivePolicyOverrideRequiredCodeRule("adaptive policy override needs an entry"),
                new AdaptivePolicyOverrideAllowedChecksumRule(new List<string>()),
            });
    }
}
