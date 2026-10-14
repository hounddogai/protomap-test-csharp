namespace Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>A rule a capped rate proposal must pass. Check returns its problems, or none.</summary>
public interface ICappedRateProposalRule
{
    List<string> Check(CappedRateProposal item);
}

public sealed record CappedRateProposalEndsAtRangeRule(int Min, int Max) : ICappedRateProposalRule
{
    public List<string> Check(CappedRateProposal item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedRateProposalRequiredContactEmailRule(string Message) : ICappedRateProposalRule
{
    public List<string> Check(CappedRateProposal item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRateProposalRequiredNameRule(string Message) : ICappedRateProposalRule
{
    public List<string> Check(CappedRateProposal item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped rate proposal.</summary>
public sealed record CappedRateProposalValidator(List<ICappedRateProposalRule> Rules)
{
    public List<string> Validate(CappedRateProposal item)
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

    public void RequireValid(CappedRateProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRateProposal> items)
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

/// <summary>Functions of capped rate proposal rules.</summary>
public static class CappedRateProposalRules
{
    public static CappedRateProposalValidator DefaultCappedRateProposalValidator()
    {
        return new CappedRateProposalValidator(
            new List<ICappedRateProposalRule>
            {
                new CappedRateProposalEndsAtRangeRule(120, 3600),
                new CappedRateProposalRequiredContactEmailRule("capped rate proposal needs this field"),
                new CappedRateProposalRequiredNameRule("capped rate proposal needs a value"),
            });
    }
}
