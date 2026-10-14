namespace Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>A rule a recognition proposal must pass. Check returns its problems, or none.</summary>
public interface IRecognitionProposalRule
{
    List<string> Check(RecognitionProposal item);
}

public sealed record RecognitionProposalRequiredGradeRule(string Message) : IRecognitionProposalRule
{
    public List<string> Check(RecognitionProposal item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionProposalCostRangeRule(double Min, double Max) : IRecognitionProposalRule
{
    public List<string> Check(RecognitionProposal item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionProposalCodePrefixRule(string Prefix) : IRecognitionProposalRule
{
    public List<string> Check(RecognitionProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionProposalRequiredNameRule(string Message) : IRecognitionProposalRule
{
    public List<string> Check(RecognitionProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition proposal.</summary>
public sealed record RecognitionProposalValidator(List<IRecognitionProposalRule> Rules)
{
    public List<string> Validate(RecognitionProposal item)
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

    public void RequireValid(RecognitionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionProposal> items)
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

/// <summary>Functions of recognition proposal rules.</summary>
public static class RecognitionProposalRules
{
    public static RecognitionProposalValidator DefaultRecognitionProposalValidator()
    {
        return new RecognitionProposalValidator(
            new List<IRecognitionProposalRule>
            {
                new RecognitionProposalRequiredGradeRule("recognition proposal needs an entry"),
                new RecognitionProposalCostRangeRule(4.1, 2406.5),
                new RecognitionProposalCodePrefixRule("REF"),
                new RecognitionProposalRequiredNameRule("recognition proposal needs a value"),
            });
    }
}
