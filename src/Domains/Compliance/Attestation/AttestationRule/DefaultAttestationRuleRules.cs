namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

/// <summary>A rule a default attestation rule must pass. Check returns its problems, or none.</summary>
public interface IDefaultAttestationRuleRule
{
    List<string> Check(DefaultAttestationRule item);
}

public sealed record DefaultAttestationRuleCodePrefixRule(string Prefix) : IDefaultAttestationRuleRule
{
    public List<string> Check(DefaultAttestationRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationRuleNameLengthRule(int MaxLength) : IDefaultAttestationRuleRule
{
    public List<string> Check(DefaultAttestationRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationRuleBudgetRangeRule(double Min, double Max) : IDefaultAttestationRuleRule
{
    public List<string> Check(DefaultAttestationRule item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultAttestationRuleCategoryLengthRule(int MaxLength) : IDefaultAttestationRuleRule
{
    public List<string> Check(DefaultAttestationRule item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationRuleExpiresAtRangeRule(int Min, int Max) : IDefaultAttestationRuleRule
{
    public List<string> Check(DefaultAttestationRule item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAttestationRuleCodeLengthRule(int MaxLength) : IDefaultAttestationRuleRule
{
    public List<string> Check(DefaultAttestationRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default attestation rule.</summary>
public sealed record DefaultAttestationRuleValidator(List<IDefaultAttestationRuleRule> Rules)
{
    public List<string> Validate(DefaultAttestationRule item)
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

    public void RequireValid(DefaultAttestationRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAttestationRule> items)
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

/// <summary>Functions of default attestation rule rules.</summary>
public static class DefaultAttestationRuleRules
{
    public static DefaultAttestationRuleValidator DefaultDefaultAttestationRuleValidator()
    {
        return new DefaultAttestationRuleValidator(
            new List<IDefaultAttestationRuleRule>
            {
                new DefaultAttestationRuleCodePrefixRule("Q"),
                new DefaultAttestationRuleNameLengthRule(3600),
                new DefaultAttestationRuleBudgetRangeRule(8.3, 3087.6),
                new DefaultAttestationRuleCategoryLengthRule(3600),
                new DefaultAttestationRuleExpiresAtRangeRule(32, 80),
                new DefaultAttestationRuleCodeLengthRule(64),
            });
    }
}
