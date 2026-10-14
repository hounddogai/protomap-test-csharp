namespace Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>A rule a deduction proposal must pass. Check returns its problems, or none.</summary>
public interface IDeductionProposalRule
{
    List<string> Check(DeductionProposal item);
}

public sealed record DeductionProposalNameLengthRule(int MaxLength) : IDeductionProposalRule
{
    public List<string> Check(DeductionProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionProposalRequiredCodeRule(string Message) : IDeductionProposalRule
{
    public List<string> Check(DeductionProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionProposalRequiredGradeRule(string Message) : IDeductionProposalRule
{
    public List<string> Check(DeductionProposal item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction proposal.</summary>
public sealed record DeductionProposalValidator(List<IDeductionProposalRule> Rules)
{
    public List<string> Validate(DeductionProposal item)
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

    public void RequireValid(DeductionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionProposal> items)
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

/// <summary>Functions of deduction proposal rules.</summary>
public static class DeductionProposalRules
{
    public static DeductionProposalValidator DefaultDeductionProposalValidator()
    {
        return new DeductionProposalValidator(
            new List<IDeductionProposalRule>
            {
                new DeductionProposalNameLengthRule(32),
                new DeductionProposalRequiredCodeRule("deduction proposal needs an entry"),
                new DeductionProposalRequiredGradeRule("deduction proposal needs an entry"),
            });
    }
}
