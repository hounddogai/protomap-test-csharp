namespace Acme.Workforce.Assets.Warranty.Digest;

/// <summary>A rule a warranty digest must pass. Check returns its problems, or none.</summary>
public interface IWarrantyDigestRule
{
    List<string> Check(WarrantyDigest item);
}

public sealed record WarrantyDigestRequiredCodeRule(string Message) : IWarrantyDigestRule
{
    public List<string> Check(WarrantyDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyDigestCostCenterLengthRule(int MaxLength) : IWarrantyDigestRule
{
    public List<string> Check(WarrantyDigest item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyDigestCodePrefixRule(string Prefix) : IWarrantyDigestRule
{
    public List<string> Check(WarrantyDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyDigestRequiredCostCenterRule(string Message) : IWarrantyDigestRule
{
    public List<string> Check(WarrantyDigest item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyDigestRequiredSkillRule(string Message) : IWarrantyDigestRule
{
    public List<string> Check(WarrantyDigest item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyDigestRequiredLabelRule(string Message) : IWarrantyDigestRule
{
    public List<string> Check(WarrantyDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty digest.</summary>
public sealed record WarrantyDigestValidator(List<IWarrantyDigestRule> Rules)
{
    public List<string> Validate(WarrantyDigest item)
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

    public void RequireValid(WarrantyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyDigest> items)
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

/// <summary>Functions of warranty digest rules.</summary>
public static class WarrantyDigestRules
{
    public static WarrantyDigestValidator DefaultWarrantyDigestValidator()
    {
        return new WarrantyDigestValidator(
            new List<IWarrantyDigestRule>
            {
                new WarrantyDigestRequiredCodeRule("warranty digest needs an entry"),
                new WarrantyDigestCostCenterLengthRule(3600),
                new WarrantyDigestCodePrefixRule("X"),
                new WarrantyDigestRequiredCostCenterRule("warranty digest needs an entry"),
                new WarrantyDigestRequiredSkillRule("warranty digest needs this field"),
                new WarrantyDigestRequiredLabelRule("warranty digest needs a value"),
            });
    }
}
