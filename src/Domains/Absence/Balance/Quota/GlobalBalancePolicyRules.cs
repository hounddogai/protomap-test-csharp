namespace Acme.Workforce.Absence.Balance.Quota;

/// <summary>A rule a global balance policy must pass. Check returns its problems, or none.</summary>
public interface IGlobalBalancePolicyRule
{
    List<string> Check(GlobalBalancePolicy item);
}

public sealed record GlobalBalancePolicyRequiredCodeRule(string Message) : IGlobalBalancePolicyRule
{
    public List<string> Check(GlobalBalancePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBalancePolicyRequiredContactEmailRule(string Message) : IGlobalBalancePolicyRule
{
    public List<string> Check(GlobalBalancePolicy item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalBalancePolicyVisibleGuardRule(int Limit) : IGlobalBalancePolicyRule
{
    public List<string> Check(GlobalBalancePolicy item)
    {
        if (item.Visible && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBalancePolicyCodePrefixRule(string Prefix) : IGlobalBalancePolicyRule
{
    public List<string> Check(GlobalBalancePolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global balance policy.</summary>
public sealed record GlobalBalancePolicyValidator(List<IGlobalBalancePolicyRule> Rules)
{
    public List<string> Validate(GlobalBalancePolicy item)
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

    public void RequireValid(GlobalBalancePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBalancePolicy> items)
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

/// <summary>Functions of global balance policy rules.</summary>
public static class GlobalBalancePolicyRules
{
    public static GlobalBalancePolicyValidator DefaultGlobalBalancePolicyValidator()
    {
        return new GlobalBalancePolicyValidator(
            new List<IGlobalBalancePolicyRule>
            {
                new GlobalBalancePolicyRequiredCodeRule("global balance policy needs an entry"),
                new GlobalBalancePolicyRequiredContactEmailRule("global balance policy needs this field"),
                new GlobalBalancePolicyVisibleGuardRule(32),
                new GlobalBalancePolicyCodePrefixRule("X"),
            });
    }
}
