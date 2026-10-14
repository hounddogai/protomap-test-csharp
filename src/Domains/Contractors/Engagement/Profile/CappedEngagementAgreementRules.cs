namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped engagement agreement must pass. Check returns its problems, or none.</summary>
public interface ICappedEngagementAgreementRule
{
    List<string> Check(CappedEngagementAgreement item);
}

public sealed record CappedEngagementAgreementRequiredContactEmailRule(string Message) : ICappedEngagementAgreementRule
{
    public List<string> Check(CappedEngagementAgreement item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementAgreementAllowedNameRule(List<string> Allowed) : ICappedEngagementAgreementRule
{
    public List<string> Check(CappedEngagementAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record CappedEngagementAgreementArchivedGuardRule(int Limit) : ICappedEngagementAgreementRule
{
    public List<string> Check(CappedEngagementAgreement item)
    {
        if (item.Archived && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped engagement agreement.</summary>
public sealed record CappedEngagementAgreementValidator(List<ICappedEngagementAgreementRule> Rules)
{
    public List<string> Validate(CappedEngagementAgreement item)
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

    public void RequireValid(CappedEngagementAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedEngagementAgreement> items)
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

/// <summary>Functions of capped engagement agreement rules.</summary>
public static class CappedEngagementAgreementRules
{
    public static CappedEngagementAgreementValidator DefaultCappedEngagementAgreementValidator()
    {
        return new CappedEngagementAgreementValidator(
            new List<ICappedEngagementAgreementRule>
            {
                new CappedEngagementAgreementRequiredContactEmailRule("capped engagement agreement needs an entry"),
                new CappedEngagementAgreementAllowedNameRule(new List<string> { "fallback" }),
                new CappedEngagementAgreementArchivedGuardRule(120),
            });
    }
}
