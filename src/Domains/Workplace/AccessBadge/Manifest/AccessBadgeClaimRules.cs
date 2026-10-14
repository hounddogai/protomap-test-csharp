namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge claim must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeClaimRule
{
    List<string> Check(AccessBadgeClaim item);
}

public sealed record AccessBadgeClaimAllowedTeamRule(List<string> Allowed) : IAccessBadgeClaimRule
{
    public List<string> Check(AccessBadgeClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeClaimRequiredContactEmailRule(string Message) : IAccessBadgeClaimRule
{
    public List<string> Check(AccessBadgeClaim item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeClaimLeaveDaysRangeRule(int Min, int Max) : IAccessBadgeClaimRule
{
    public List<string> Check(AccessBadgeClaim item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeClaimMarginRangeRule(double Min, double Max) : IAccessBadgeClaimRule
{
    public List<string> Check(AccessBadgeClaim item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge claim.</summary>
public sealed record AccessBadgeClaimValidator(List<IAccessBadgeClaimRule> Rules)
{
    public List<string> Validate(AccessBadgeClaim item)
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

    public void RequireValid(AccessBadgeClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeClaim> items)
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

/// <summary>Functions of access badge claim rules.</summary>
public static class AccessBadgeClaimRules
{
    public static AccessBadgeClaimValidator DefaultAccessBadgeClaimValidator()
    {
        return new AccessBadgeClaimValidator(
            new List<IAccessBadgeClaimRule>
            {
                new AccessBadgeClaimAllowedTeamRule(new List<string>()),
                new AccessBadgeClaimRequiredContactEmailRule("access badge claim needs this field"),
                new AccessBadgeClaimLeaveDaysRangeRule(16, 120),
                new AccessBadgeClaimMarginRangeRule(4.6, 3919.1),
            });
    }
}
