namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>A rule a access badge agreement must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeAgreementRule
{
    List<string> Check(AccessBadgeAgreement item);
}

public sealed record AccessBadgeAgreementRequiredNameRule(string Message) : IAccessBadgeAgreementRule
{
    public List<string> Check(AccessBadgeAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAgreementManagerIdLengthRule(int MaxLength) : IAccessBadgeAgreementRule
{
    public List<string> Check(AccessBadgeAgreement item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAgreementCodeLengthRule(int MaxLength) : IAccessBadgeAgreementRule
{
    public List<string> Check(AccessBadgeAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge agreement.</summary>
public sealed record AccessBadgeAgreementValidator(List<IAccessBadgeAgreementRule> Rules)
{
    public List<string> Validate(AccessBadgeAgreement item)
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

    public void RequireValid(AccessBadgeAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeAgreement> items)
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

/// <summary>Functions of access badge agreement rules.</summary>
public static class AccessBadgeAgreementRules
{
    public static AccessBadgeAgreementValidator DefaultAccessBadgeAgreementValidator()
    {
        return new AccessBadgeAgreementValidator(
            new List<IAccessBadgeAgreementRule>
            {
                new AccessBadgeAgreementRequiredNameRule("access badge agreement needs a value"),
                new AccessBadgeAgreementManagerIdLengthRule(86400),
                new AccessBadgeAgreementCodeLengthRule(86400),
            });
    }
}
