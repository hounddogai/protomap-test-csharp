namespace Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>A rule a desk proposal must pass. Check returns its problems, or none.</summary>
public interface IDeskProposalRule
{
    List<string> Check(DeskProposal item);
}

public sealed record DeskProposalSalaryBandLengthRule(int MaxLength) : IDeskProposalRule
{
    public List<string> Check(DeskProposal item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskProposalCodeLengthRule(int MaxLength) : IDeskProposalRule
{
    public List<string> Check(DeskProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskProposalRequiredTitleRule(string Message) : IDeskProposalRule
{
    public List<string> Check(DeskProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk proposal.</summary>
public sealed record DeskProposalValidator(List<IDeskProposalRule> Rules)
{
    public List<string> Validate(DeskProposal item)
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

    public void RequireValid(DeskProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskProposal> items)
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

/// <summary>Functions of desk proposal rules.</summary>
public static class DeskProposalRules
{
    public static DeskProposalValidator DefaultDeskProposalValidator()
    {
        return new DeskProposalValidator(
            new List<IDeskProposalRule>
            {
                new DeskProposalSalaryBandLengthRule(255),
                new DeskProposalCodeLengthRule(64),
                new DeskProposalRequiredTitleRule("desk proposal needs an entry"),
            });
    }
}
