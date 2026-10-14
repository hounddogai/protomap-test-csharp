namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual headcount rule must pass. Check returns its problems, or none.</summary>
public interface IManualHeadcountRuleRule
{
    List<string> Check(ManualHeadcountRule item);
}

public sealed record ManualHeadcountRuleOwnerIdLengthRule(int MaxLength) : IManualHeadcountRuleRule
{
    public List<string> Check(ManualHeadcountRule item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualHeadcountRuleCodePrefixRule(string Prefix) : IManualHeadcountRuleRule
{
    public List<string> Check(ManualHeadcountRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualHeadcountRuleAllowedSalaryBandRule(List<string> Allowed) : IManualHeadcountRuleRule
{
    public List<string> Check(ManualHeadcountRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual headcount rule.</summary>
public sealed record ManualHeadcountRuleValidator(List<IManualHeadcountRuleRule> Rules)
{
    public List<string> Validate(ManualHeadcountRule item)
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

    public void RequireValid(ManualHeadcountRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualHeadcountRule> items)
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

/// <summary>Functions of manual headcount rule rules.</summary>
public static class ManualHeadcountRuleRules
{
    public static ManualHeadcountRuleValidator DefaultManualHeadcountRuleValidator()
    {
        return new ManualHeadcountRuleValidator(
            new List<IManualHeadcountRuleRule>
            {
                new ManualHeadcountRuleOwnerIdLengthRule(80),
                new ManualHeadcountRuleCodePrefixRule("X"),
                new ManualHeadcountRuleAllowedSalaryBandRule(new List<string> { "global", "seasonal", "adaptive" }),
            });
    }
}
