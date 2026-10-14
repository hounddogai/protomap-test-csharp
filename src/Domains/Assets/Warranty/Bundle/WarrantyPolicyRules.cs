namespace Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>A rule a warranty policy must pass. Check returns its problems, or none.</summary>
public interface IWarrantyPolicyRule
{
    List<string> Check(WarrantyPolicy item);
}

public sealed record WarrantyPolicyCodePrefixRule(string Prefix) : IWarrantyPolicyRule
{
    public List<string> Check(WarrantyPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyPolicyLabelLengthRule(int MaxLength) : IWarrantyPolicyRule
{
    public List<string> Check(WarrantyPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyPolicyRequiredDescriptionRule(string Message) : IWarrantyPolicyRule
{
    public List<string> Check(WarrantyPolicy item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty policy.</summary>
public sealed record WarrantyPolicyValidator(List<IWarrantyPolicyRule> Rules)
{
    public List<string> Validate(WarrantyPolicy item)
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

    public void RequireValid(WarrantyPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyPolicy> items)
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

/// <summary>Functions of warranty policy rules.</summary>
public static class WarrantyPolicyRules
{
    public static WarrantyPolicyValidator DefaultWarrantyPolicyValidator()
    {
        return new WarrantyPolicyValidator(
            new List<IWarrantyPolicyRule>
            {
                new WarrantyPolicyCodePrefixRule("X"),
                new WarrantyPolicyLabelLengthRule(3600),
                new WarrantyPolicyRequiredDescriptionRule("warranty policy needs an entry"),
            });
    }
}
