namespace Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>A rule a engagement proposal must pass. Check returns its problems, or none.</summary>
public interface IEngagementProposalRule
{
    List<string> Check(EngagementProposal item);
}

public sealed record EngagementProposalManagerIdLengthRule(int MaxLength) : IEngagementProposalRule
{
    public List<string> Check(EngagementProposal item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementProposalRequiredNameRule(string Message) : IEngagementProposalRule
{
    public List<string> Check(EngagementProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementProposalRequiredManagerIdRule(string Message) : IEngagementProposalRule
{
    public List<string> Check(EngagementProposal item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement proposal.</summary>
public sealed record EngagementProposalValidator(List<IEngagementProposalRule> Rules)
{
    public List<string> Validate(EngagementProposal item)
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

    public void RequireValid(EngagementProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementProposal> items)
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

/// <summary>Functions of engagement proposal rules.</summary>
public static class EngagementProposalRules
{
    public static EngagementProposalValidator DefaultEngagementProposalValidator()
    {
        return new EngagementProposalValidator(
            new List<IEngagementProposalRule>
            {
                new EngagementProposalManagerIdLengthRule(255),
                new EngagementProposalRequiredNameRule("engagement proposal needs this field"),
                new EngagementProposalRequiredManagerIdRule("engagement proposal needs a value"),
            });
    }
}
