namespace Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>A rule a feedback proposal must pass. Check returns its problems, or none.</summary>
public interface IFeedbackProposalRule
{
    List<string> Check(FeedbackProposal item);
}

public sealed record FeedbackProposalCodeLengthRule(int MaxLength) : IFeedbackProposalRule
{
    public List<string> Check(FeedbackProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackProposalRequiredTitleRule(string Message) : IFeedbackProposalRule
{
    public List<string> Check(FeedbackProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackProposalRequiredCodeRule(string Message) : IFeedbackProposalRule
{
    public List<string> Check(FeedbackProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback proposal.</summary>
public sealed record FeedbackProposalValidator(List<IFeedbackProposalRule> Rules)
{
    public List<string> Validate(FeedbackProposal item)
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

    public void RequireValid(FeedbackProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackProposal> items)
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

/// <summary>Functions of feedback proposal rules.</summary>
public static class FeedbackProposalRules
{
    public static FeedbackProposalValidator DefaultFeedbackProposalValidator()
    {
        return new FeedbackProposalValidator(
            new List<IFeedbackProposalRule>
            {
                new FeedbackProposalCodeLengthRule(16),
                new FeedbackProposalRequiredTitleRule("feedback proposal needs an entry"),
                new FeedbackProposalRequiredCodeRule("feedback proposal needs an entry"),
            });
    }
}
