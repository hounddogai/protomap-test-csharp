namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling access badge agreement must pass. Check returns its problems, or none.</summary>
public interface IRollingAccessBadgeAgreementRule
{
    List<string> Check(RollingAccessBadgeAgreement item);
}

public sealed record RollingAccessBadgeAgreementAllowedTimezoneRule(List<string> Allowed) : IRollingAccessBadgeAgreementRule
{
    public List<string> Check(RollingAccessBadgeAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeAgreementCodeLengthRule(int MaxLength) : IRollingAccessBadgeAgreementRule
{
    public List<string> Check(RollingAccessBadgeAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeAgreementCodePrefixRule(string Prefix) : IRollingAccessBadgeAgreementRule
{
    public List<string> Check(RollingAccessBadgeAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling access badge agreement.</summary>
public sealed record RollingAccessBadgeAgreementValidator(List<IRollingAccessBadgeAgreementRule> Rules)
{
    public List<string> Validate(RollingAccessBadgeAgreement item)
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

    public void RequireValid(RollingAccessBadgeAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccessBadgeAgreement> items)
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

/// <summary>Functions of rolling access badge agreement rules.</summary>
public static class RollingAccessBadgeAgreementRules
{
    public static RollingAccessBadgeAgreementValidator DefaultRollingAccessBadgeAgreementValidator()
    {
        return new RollingAccessBadgeAgreementValidator(
            new List<IRollingAccessBadgeAgreementRule>
            {
                new RollingAccessBadgeAgreementAllowedTimezoneRule(new List<string>()),
                new RollingAccessBadgeAgreementCodeLengthRule(64),
                new RollingAccessBadgeAgreementCodePrefixRule("ACM"),
            });
    }
}
