namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>A rule a capped statement of work proposal must pass. Check returns its problems, or none.</summary>
public interface ICappedStatementOfWorkProposalRule
{
    List<string> Check(CappedStatementOfWorkProposal item);
}

public sealed record CappedStatementOfWorkProposalDiscountRangeRule(double Min, double Max) : ICappedStatementOfWorkProposalRule
{
    public List<string> Check(CappedStatementOfWorkProposal item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedStatementOfWorkProposalCodePrefixRule(string Prefix) : ICappedStatementOfWorkProposalRule
{
    public List<string> Check(CappedStatementOfWorkProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedStatementOfWorkProposalRequiredCodeRule(string Message) : ICappedStatementOfWorkProposalRule
{
    public List<string> Check(CappedStatementOfWorkProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped statement of work proposal.</summary>
public sealed record CappedStatementOfWorkProposalValidator(List<ICappedStatementOfWorkProposalRule> Rules)
{
    public List<string> Validate(CappedStatementOfWorkProposal item)
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

    public void RequireValid(CappedStatementOfWorkProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedStatementOfWorkProposal> items)
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

/// <summary>Functions of capped statement of work proposal rules.</summary>
public static class CappedStatementOfWorkProposalRules
{
    public static CappedStatementOfWorkProposalValidator DefaultCappedStatementOfWorkProposalValidator()
    {
        return new CappedStatementOfWorkProposalValidator(
            new List<ICappedStatementOfWorkProposalRule>
            {
                new CappedStatementOfWorkProposalDiscountRangeRule(1.9, 2591.7),
                new CappedStatementOfWorkProposalCodePrefixRule("OPS"),
                new CappedStatementOfWorkProposalRequiredCodeRule(
                    "capped statement of work proposal needs this field"),
            });
    }
}
