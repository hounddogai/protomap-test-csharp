namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>A rule a headcount proposal must pass. Check returns its problems, or none.</summary>
public interface IHeadcountProposalRule
{
    List<string> Check(HeadcountProposal item);
}

public sealed record HeadcountProposalTenureMonthsRangeRule(int Min, int Max) : IHeadcountProposalRule
{
    public List<string> Check(HeadcountProposal item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProposalCodePrefixRule(string Prefix) : IHeadcountProposalRule
{
    public List<string> Check(HeadcountProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProposalRequiredCodeRule(string Message) : IHeadcountProposalRule
{
    public List<string> Check(HeadcountProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProposalRequiredSkillRule(string Message) : IHeadcountProposalRule
{
    public List<string> Check(HeadcountProposal item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProposalRequiredTitleRule(string Message) : IHeadcountProposalRule
{
    public List<string> Check(HeadcountProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountProposalUpdatedAtRangeRule(int Min, int Max) : IHeadcountProposalRule
{
    public List<string> Check(HeadcountProposal item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount proposal.</summary>
public sealed record HeadcountProposalValidator(List<IHeadcountProposalRule> Rules)
{
    public List<string> Validate(HeadcountProposal item)
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

    public void RequireValid(HeadcountProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountProposal> items)
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

/// <summary>Functions of headcount proposal rules.</summary>
public static class HeadcountProposalRules
{
    public static HeadcountProposalValidator DefaultHeadcountProposalValidator()
    {
        return new HeadcountProposalValidator(
            new List<IHeadcountProposalRule>
            {
                new HeadcountProposalTenureMonthsRangeRule(16, 64),
                new HeadcountProposalCodePrefixRule("OPS"),
                new HeadcountProposalRequiredCodeRule("headcount proposal needs an entry"),
                new HeadcountProposalRequiredSkillRule("headcount proposal needs a value"),
                new HeadcountProposalRequiredTitleRule("headcount proposal needs this field"),
                new HeadcountProposalUpdatedAtRangeRule(120, 3600),
            });
    }
}
