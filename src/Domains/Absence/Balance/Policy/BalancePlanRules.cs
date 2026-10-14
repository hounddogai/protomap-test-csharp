namespace Acme.Workforce.Absence.Balance.Policy;

/// <summary>A rule a balance plan must pass. Check returns its problems, or none.</summary>
public interface IBalancePlanRule
{
    List<string> Check(BalancePlan item);
}

public sealed record BalancePlanCodePrefixRule(string Prefix) : IBalancePlanRule
{
    public List<string> Check(BalancePlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalancePlanRequiredCodeRule(string Message) : IBalancePlanRule
{
    public List<string> Check(BalancePlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalancePlanTitleLengthRule(int MaxLength) : IBalancePlanRule
{
    public List<string> Check(BalancePlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalancePlanRequiredContactEmailRule(string Message) : IBalancePlanRule
{
    public List<string> Check(BalancePlan item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance plan.</summary>
public sealed record BalancePlanValidator(List<IBalancePlanRule> Rules)
{
    public List<string> Validate(BalancePlan item)
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

    public void RequireValid(BalancePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalancePlan> items)
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

/// <summary>Functions of balance plan rules.</summary>
public static class BalancePlanRules
{
    public static BalancePlanValidator DefaultBalancePlanValidator()
    {
        return new BalancePlanValidator(
            new List<IBalancePlanRule>
            {
                new BalancePlanCodePrefixRule("ACM"),
                new BalancePlanRequiredCodeRule("balance plan needs this field"),
                new BalancePlanTitleLengthRule(80),
                new BalancePlanRequiredContactEmailRule("balance plan needs a value"),
            });
    }
}
