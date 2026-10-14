namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional balance policy must pass. Check returns its problems, or none.</summary>
public interface IRegionalBalancePolicyRule
{
    List<string> Check(RegionalBalancePolicy item);
}

public sealed record RegionalBalancePolicyScoreRangeRule(double Min, double Max) : IRegionalBalancePolicyRule
{
    public List<string> Check(RegionalBalancePolicy item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalancePolicyAllowedDepartmentRule(List<string> Allowed) : IRegionalBalancePolicyRule
{
    public List<string> Check(RegionalBalancePolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalancePolicyReviewerLengthRule(int MaxLength) : IRegionalBalancePolicyRule
{
    public List<string> Check(RegionalBalancePolicy item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalBalancePolicyRetriesRangeRule(int Min, int Max) : IRegionalBalancePolicyRule
{
    public List<string> Check(RegionalBalancePolicy item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional balance policy.</summary>
public sealed record RegionalBalancePolicyValidator(List<IRegionalBalancePolicyRule> Rules)
{
    public List<string> Validate(RegionalBalancePolicy item)
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

    public void RequireValid(RegionalBalancePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalBalancePolicy> items)
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

/// <summary>Functions of regional balance policy rules.</summary>
public static class RegionalBalancePolicyRules
{
    public static RegionalBalancePolicyValidator DefaultRegionalBalancePolicyValidator()
    {
        return new RegionalBalancePolicyValidator(
            new List<IRegionalBalancePolicyRule>
            {
                new RegionalBalancePolicyScoreRangeRule(6.7, 2747.4),
                new RegionalBalancePolicyAllowedDepartmentRule(new List<string> { "pending", "capped" }),
                new RegionalBalancePolicyReviewerLengthRule(1000),
                new RegionalBalancePolicyRetriesRangeRule(64, 3600),
            });
    }
}
