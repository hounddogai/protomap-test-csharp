namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>A rule a regional feedback proposal must pass. Check returns its problems, or none.</summary>
public interface IRegionalFeedbackProposalRule
{
    List<string> Check(RegionalFeedbackProposal item);
}

public sealed record RegionalFeedbackProposalLabelLengthRule(int MaxLength) : IRegionalFeedbackProposalRule
{
    public List<string> Check(RegionalFeedbackProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalFeedbackProposalLimitRangeRule(int Min, int Max) : IRegionalFeedbackProposalRule
{
    public List<string> Check(RegionalFeedbackProposal item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalFeedbackProposalUpdatedAtRangeRule(int Min, int Max) : IRegionalFeedbackProposalRule
{
    public List<string> Check(RegionalFeedbackProposal item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional feedback proposal.</summary>
public sealed record RegionalFeedbackProposalValidator(List<IRegionalFeedbackProposalRule> Rules)
{
    public List<string> Validate(RegionalFeedbackProposal item)
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

    public void RequireValid(RegionalFeedbackProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalFeedbackProposal> items)
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

/// <summary>Functions of regional feedback proposal rules.</summary>
public static class RegionalFeedbackProposalRules
{
    public static RegionalFeedbackProposalValidator DefaultRegionalFeedbackProposalValidator()
    {
        return new RegionalFeedbackProposalValidator(
            new List<IRegionalFeedbackProposalRule>
            {
                new RegionalFeedbackProposalLabelLengthRule(120),
                new RegionalFeedbackProposalLimitRangeRule(16, 3600),
                new RegionalFeedbackProposalUpdatedAtRangeRule(255, 3600),
            });
    }
}
