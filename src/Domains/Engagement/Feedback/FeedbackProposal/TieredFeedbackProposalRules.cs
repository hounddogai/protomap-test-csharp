namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>A rule a tiered feedback proposal must pass. Check returns its problems, or none.</summary>
public interface ITieredFeedbackProposalRule
{
    List<string> Check(TieredFeedbackProposal item);
}

public sealed record TieredFeedbackProposalNameLengthRule(int MaxLength) : ITieredFeedbackProposalRule
{
    public List<string> Check(TieredFeedbackProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackProposalCodeLengthRule(int MaxLength) : ITieredFeedbackProposalRule
{
    public List<string> Check(TieredFeedbackProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredFeedbackProposalHoursRangeRule(double Min, double Max) : ITieredFeedbackProposalRule
{
    public List<string> Check(TieredFeedbackProposal item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredFeedbackProposalOwnerIdLengthRule(int MaxLength) : ITieredFeedbackProposalRule
{
    public List<string> Check(TieredFeedbackProposal item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered feedback proposal.</summary>
public sealed record TieredFeedbackProposalValidator(List<ITieredFeedbackProposalRule> Rules)
{
    public List<string> Validate(TieredFeedbackProposal item)
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

    public void RequireValid(TieredFeedbackProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredFeedbackProposal> items)
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

/// <summary>Functions of tiered feedback proposal rules.</summary>
public static class TieredFeedbackProposalRules
{
    public static TieredFeedbackProposalValidator DefaultTieredFeedbackProposalValidator()
    {
        return new TieredFeedbackProposalValidator(
            new List<ITieredFeedbackProposalRule>
            {
                new TieredFeedbackProposalNameLengthRule(64),
                new TieredFeedbackProposalCodeLengthRule(16),
                new TieredFeedbackProposalHoursRangeRule(10.0, 593.5),
                new TieredFeedbackProposalOwnerIdLengthRule(64),
            });
    }
}
