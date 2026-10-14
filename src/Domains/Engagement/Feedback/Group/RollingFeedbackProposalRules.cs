namespace Acme.Workforce.Engagement.Feedback.Group;

/// <summary>A rule a rolling feedback proposal must pass. Check returns its problems, or none.</summary>
public interface IRollingFeedbackProposalRule
{
    List<string> Check(RollingFeedbackProposal item);
}

public sealed record RollingFeedbackProposalRatioRangeRule(double Min, double Max) : IRollingFeedbackProposalRule
{
    public List<string> Check(RollingFeedbackProposal item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingFeedbackProposalRatingRangeRule(double Min, double Max) : IRollingFeedbackProposalRule
{
    public List<string> Check(RollingFeedbackProposal item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingFeedbackProposalRequiredExternalRefRule(string Message) : IRollingFeedbackProposalRule
{
    public List<string> Check(RollingFeedbackProposal item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling feedback proposal.</summary>
public sealed record RollingFeedbackProposalValidator(List<IRollingFeedbackProposalRule> Rules)
{
    public List<string> Validate(RollingFeedbackProposal item)
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

    public void RequireValid(RollingFeedbackProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingFeedbackProposal> items)
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

/// <summary>Functions of rolling feedback proposal rules.</summary>
public static class RollingFeedbackProposalRules
{
    public static RollingFeedbackProposalValidator DefaultRollingFeedbackProposalValidator()
    {
        return new RollingFeedbackProposalValidator(
            new List<IRollingFeedbackProposalRule>
            {
                new RollingFeedbackProposalRatioRangeRule(0.7, 1239.0),
                new RollingFeedbackProposalRatingRangeRule(0.1, 2986.2),
                new RollingFeedbackProposalRequiredExternalRefRule("rolling feedback proposal needs this field"),
            });
    }
}
