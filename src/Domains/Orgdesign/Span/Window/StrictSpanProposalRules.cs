namespace Acme.Workforce.Orgdesign.Span.Window;

/// <summary>A rule a strict span proposal must pass. Check returns its problems, or none.</summary>
public interface IStrictSpanProposalRule
{
    List<string> Check(StrictSpanProposal item);
}

public sealed record StrictSpanProposalSkillLengthRule(int MaxLength) : IStrictSpanProposalRule
{
    public List<string> Check(StrictSpanProposal item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSpanProposalCodeLengthRule(int MaxLength) : IStrictSpanProposalRule
{
    public List<string> Check(StrictSpanProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSpanProposalRequiredCodeRule(string Message) : IStrictSpanProposalRule
{
    public List<string> Check(StrictSpanProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict span proposal.</summary>
public sealed record StrictSpanProposalValidator(List<IStrictSpanProposalRule> Rules)
{
    public List<string> Validate(StrictSpanProposal item)
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

    public void RequireValid(StrictSpanProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSpanProposal> items)
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

/// <summary>Functions of strict span proposal rules.</summary>
public static class StrictSpanProposalRules
{
    public static StrictSpanProposalValidator DefaultStrictSpanProposalValidator()
    {
        return new StrictSpanProposalValidator(
            new List<IStrictSpanProposalRule>
            {
                new StrictSpanProposalSkillLengthRule(64),
                new StrictSpanProposalCodeLengthRule(255),
                new StrictSpanProposalRequiredCodeRule("strict span proposal needs an entry"),
            });
    }
}
