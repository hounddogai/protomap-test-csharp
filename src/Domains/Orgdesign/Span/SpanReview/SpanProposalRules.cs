namespace Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>A rule a span proposal must pass. Check returns its problems, or none.</summary>
public interface ISpanProposalRule
{
    List<string> Check(SpanProposal item);
}

public sealed record SpanProposalSalaryBandLengthRule(int MaxLength) : ISpanProposalRule
{
    public List<string> Check(SpanProposal item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanProposalAmountRangeRule(double Min, double Max) : ISpanProposalRule
{
    public List<string> Check(SpanProposal item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanProposalScoreRangeRule(double Min, double Max) : ISpanProposalRule
{
    public List<string> Check(SpanProposal item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanProposalRequiredChecksumRule(string Message) : ISpanProposalRule
{
    public List<string> Check(SpanProposal item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span proposal.</summary>
public sealed record SpanProposalValidator(List<ISpanProposalRule> Rules)
{
    public List<string> Validate(SpanProposal item)
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

    public void RequireValid(SpanProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanProposal> items)
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

/// <summary>Functions of span proposal rules.</summary>
public static class SpanProposalRules
{
    public static SpanProposalValidator DefaultSpanProposalValidator()
    {
        return new SpanProposalValidator(
            new List<ISpanProposalRule>
            {
                new SpanProposalSalaryBandLengthRule(1000),
                new SpanProposalAmountRangeRule(8.0, 3357.6),
                new SpanProposalScoreRangeRule(4.4, 569.3),
                new SpanProposalRequiredChecksumRule("span proposal needs a value"),
            });
    }
}
