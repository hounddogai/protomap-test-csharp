namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan claim must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanClaimRule
{
    List<string> Check(SuccessionPlanClaim item);
}

public sealed record SuccessionPlanClaimAmountRangeRule(double Min, double Max) : ISuccessionPlanClaimRule
{
    public List<string> Check(SuccessionPlanClaim item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanClaimRequiredTitleRule(string Message) : ISuccessionPlanClaimRule
{
    public List<string> Check(SuccessionPlanClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanClaimCodePrefixRule(string Prefix) : ISuccessionPlanClaimRule
{
    public List<string> Check(SuccessionPlanClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanClaimAllowedTitleRule(List<string> Allowed) : ISuccessionPlanClaimRule
{
    public List<string> Check(SuccessionPlanClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan claim.</summary>
public sealed record SuccessionPlanClaimValidator(List<ISuccessionPlanClaimRule> Rules)
{
    public List<string> Validate(SuccessionPlanClaim item)
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

    public void RequireValid(SuccessionPlanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanClaim> items)
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

/// <summary>Functions of succession plan claim rules.</summary>
public static class SuccessionPlanClaimRules
{
    public static SuccessionPlanClaimValidator DefaultSuccessionPlanClaimValidator()
    {
        return new SuccessionPlanClaimValidator(
            new List<ISuccessionPlanClaimRule>
            {
                new SuccessionPlanClaimAmountRangeRule(9.0, 1035.7),
                new SuccessionPlanClaimRequiredTitleRule("succession plan claim needs an entry"),
                new SuccessionPlanClaimCodePrefixRule("Q"),
                new SuccessionPlanClaimAllowedTitleRule(new List<string> { "legacy", "seasonal", "weighted" }),
            });
    }
}
