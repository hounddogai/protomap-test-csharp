namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>A rule a strict career path proposal must pass. Check returns its problems, or none.</summary>
public interface IStrictCareerPathProposalRule
{
    List<string> Check(StrictCareerPathProposal item);
}

public sealed record StrictCareerPathProposalCodeLengthRule(int MaxLength) : IStrictCareerPathProposalRule
{
    public List<string> Check(StrictCareerPathProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathProposalRequiredTitleRule(string Message) : IStrictCareerPathProposalRule
{
    public List<string> Check(StrictCareerPathProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathProposalRequiredCountryRule(string Message) : IStrictCareerPathProposalRule
{
    public List<string> Check(StrictCareerPathProposal item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathProposalRequiredCodeRule(string Message) : IStrictCareerPathProposalRule
{
    public List<string> Check(StrictCareerPathProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict career path proposal.</summary>
public sealed record StrictCareerPathProposalValidator(List<IStrictCareerPathProposalRule> Rules)
{
    public List<string> Validate(StrictCareerPathProposal item)
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

    public void RequireValid(StrictCareerPathProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictCareerPathProposal> items)
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

/// <summary>Functions of strict career path proposal rules.</summary>
public static class StrictCareerPathProposalRules
{
    public static StrictCareerPathProposalValidator DefaultStrictCareerPathProposalValidator()
    {
        return new StrictCareerPathProposalValidator(
            new List<IStrictCareerPathProposalRule>
            {
                new StrictCareerPathProposalCodeLengthRule(32),
                new StrictCareerPathProposalRequiredTitleRule("strict career path proposal needs an entry"),
                new StrictCareerPathProposalRequiredCountryRule("strict career path proposal needs this field"),
                new StrictCareerPathProposalRequiredCodeRule("strict career path proposal needs this field"),
            });
    }
}
