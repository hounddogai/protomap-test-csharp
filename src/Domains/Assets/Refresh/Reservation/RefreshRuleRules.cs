namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh rule must pass. Check returns its problems, or none.</summary>
public interface IRefreshRuleRule
{
    List<string> Check(RefreshRule item);
}

public sealed record RefreshRuleCodePrefixRule(string Prefix) : IRefreshRuleRule
{
    public List<string> Check(RefreshRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRuleRequiredOwnerIdRule(string Message) : IRefreshRuleRule
{
    public List<string> Check(RefreshRule item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshRuleAllowedExternalRefRule(List<string> Allowed) : IRefreshRuleRule
{
    public List<string> Check(RefreshRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRuleRequiredCodeRule(string Message) : IRefreshRuleRule
{
    public List<string> Check(RefreshRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshRuleRequiredEmployeeIdRule(string Message) : IRefreshRuleRule
{
    public List<string> Check(RefreshRule item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh rule.</summary>
public sealed record RefreshRuleValidator(List<IRefreshRuleRule> Rules)
{
    public List<string> Validate(RefreshRule item)
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

    public void RequireValid(RefreshRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshRule> items)
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

/// <summary>Functions of refresh rule rules.</summary>
public static class RefreshRuleRules
{
    public static RefreshRuleValidator DefaultRefreshRuleValidator()
    {
        return new RefreshRuleValidator(
            new List<IRefreshRuleRule>
            {
                new RefreshRuleCodePrefixRule("Q"),
                new RefreshRuleRequiredOwnerIdRule("refresh rule needs this field"),
                new RefreshRuleAllowedExternalRefRule(new List<string> { "tiered", "legacy", "capped" }),
                new RefreshRuleRequiredCodeRule("refresh rule needs this field"),
                new RefreshRuleRequiredEmployeeIdRule("refresh rule needs an entry"),
            });
    }
}
