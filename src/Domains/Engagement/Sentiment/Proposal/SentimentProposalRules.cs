namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment proposal must pass. Check returns its problems, or none.</summary>
public interface ISentimentProposalRule
{
    List<string> Check(SentimentProposal item);
}

public sealed record SentimentProposalAllowedNameRule(List<string> Allowed) : ISentimentProposalRule
{
    public List<string> Check(SentimentProposal item)
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

public sealed record SentimentProposalCodeLengthRule(int MaxLength) : ISentimentProposalRule
{
    public List<string> Check(SentimentProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentProposalRequiredTitleRule(string Message) : ISentimentProposalRule
{
    public List<string> Check(SentimentProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentProposalFloorValueRangeRule(double Min, double Max) : ISentimentProposalRule
{
    public List<string> Check(SentimentProposal item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment proposal.</summary>
public sealed record SentimentProposalValidator(List<ISentimentProposalRule> Rules)
{
    public List<string> Validate(SentimentProposal item)
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

    public void RequireValid(SentimentProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentProposal> items)
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

/// <summary>Functions of sentiment proposal rules.</summary>
public static class SentimentProposalRules
{
    public static SentimentProposalValidator DefaultSentimentProposalValidator()
    {
        return new SentimentProposalValidator(
            new List<ISentimentProposalRule>
            {
                new SentimentProposalAllowedNameRule(new List<string> { "default", "legacy" }),
                new SentimentProposalCodeLengthRule(32),
                new SentimentProposalRequiredTitleRule("sentiment proposal needs a value"),
                new SentimentProposalFloorValueRangeRule(0.8, 4377.7),
            });
    }
}
