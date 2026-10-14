namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global engagement proposal must pass. Check returns its problems, or none.</summary>
public interface IGlobalEngagementProposalRule
{
    List<string> Check(GlobalEngagementProposal item);
}

public sealed record GlobalEngagementProposalLocationLengthRule(int MaxLength) : IGlobalEngagementProposalRule
{
    public List<string> Check(GlobalEngagementProposal item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEngagementProposalNameLengthRule(int MaxLength) : IGlobalEngagementProposalRule
{
    public List<string> Check(GlobalEngagementProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEngagementProposalAllowedNameRule(List<string> Allowed) : IGlobalEngagementProposalRule
{
    public List<string> Check(GlobalEngagementProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global engagement proposal.</summary>
public sealed record GlobalEngagementProposalValidator(List<IGlobalEngagementProposalRule> Rules)
{
    public List<string> Validate(GlobalEngagementProposal item)
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

    public void RequireValid(GlobalEngagementProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalEngagementProposal> items)
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

/// <summary>Functions of global engagement proposal rules.</summary>
public static class GlobalEngagementProposalRules
{
    public static GlobalEngagementProposalValidator DefaultGlobalEngagementProposalValidator()
    {
        return new GlobalEngagementProposalValidator(
            new List<IGlobalEngagementProposalRule>
            {
                new GlobalEngagementProposalLocationLengthRule(3600),
                new GlobalEngagementProposalNameLengthRule(16),
                new GlobalEngagementProposalAllowedNameRule(new List<string> { "pending", "primary" }),
            });
    }
}
