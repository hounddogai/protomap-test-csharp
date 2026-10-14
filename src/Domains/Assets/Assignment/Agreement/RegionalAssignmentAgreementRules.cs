namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional assignment agreement must pass. Check returns its problems, or none.</summary>
public interface IRegionalAssignmentAgreementRule
{
    List<string> Check(RegionalAssignmentAgreement item);
}

public sealed record RegionalAssignmentAgreementRequiredSegmentRule(string Message) : IRegionalAssignmentAgreementRule
{
    public List<string> Check(RegionalAssignmentAgreement item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentAgreementAllowedNameRule(List<string> Allowed) : IRegionalAssignmentAgreementRule
{
    public List<string> Check(RegionalAssignmentAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentAgreementRequiredCodeRule(string Message) : IRegionalAssignmentAgreementRule
{
    public List<string> Check(RegionalAssignmentAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional assignment agreement.</summary>
public sealed record RegionalAssignmentAgreementValidator(List<IRegionalAssignmentAgreementRule> Rules)
{
    public List<string> Validate(RegionalAssignmentAgreement item)
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

    public void RequireValid(RegionalAssignmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAssignmentAgreement> items)
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

/// <summary>Functions of regional assignment agreement rules.</summary>
public static class RegionalAssignmentAgreementRules
{
    public static RegionalAssignmentAgreementValidator DefaultRegionalAssignmentAgreementValidator()
    {
        return new RegionalAssignmentAgreementValidator(
            new List<IRegionalAssignmentAgreementRule>
            {
                new RegionalAssignmentAgreementRequiredSegmentRule("regional assignment agreement needs this field"),
                new RegionalAssignmentAgreementAllowedNameRule(new List<string> { "scheduled", "strict", "primary" }),
                new RegionalAssignmentAgreementRequiredCodeRule("regional assignment agreement needs an entry"),
            });
    }
}
