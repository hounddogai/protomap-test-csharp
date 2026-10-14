namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>A rule a strict retro pay proposal must pass. Check returns its problems, or none.</summary>
public interface IStrictRetroPayProposalRule
{
    List<string> Check(StrictRetroPayProposal item);
}

public sealed record StrictRetroPayProposalLabelLengthRule(int MaxLength) : IStrictRetroPayProposalRule
{
    public List<string> Check(StrictRetroPayProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRetroPayProposalCodeLengthRule(int MaxLength) : IStrictRetroPayProposalRule
{
    public List<string> Check(StrictRetroPayProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRetroPayProposalRequiredCategoryRule(string Message) : IStrictRetroPayProposalRule
{
    public List<string> Check(StrictRetroPayProposal item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict retro pay proposal.</summary>
public sealed record StrictRetroPayProposalValidator(List<IStrictRetroPayProposalRule> Rules)
{
    public List<string> Validate(StrictRetroPayProposal item)
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

    public void RequireValid(StrictRetroPayProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictRetroPayProposal> items)
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

/// <summary>Functions of strict retro pay proposal rules.</summary>
public static class StrictRetroPayProposalRules
{
    public static StrictRetroPayProposalValidator DefaultStrictRetroPayProposalValidator()
    {
        return new StrictRetroPayProposalValidator(
            new List<IStrictRetroPayProposalRule>
            {
                new StrictRetroPayProposalLabelLengthRule(64),
                new StrictRetroPayProposalCodeLengthRule(86400),
                new StrictRetroPayProposalRequiredCategoryRule("strict retro pay proposal needs a value"),
            });
    }
}
