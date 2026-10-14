namespace Acme.Workforce.Payrollops.PayRun.Budget;

/// <summary>A rule a pay run proposal must pass. Check returns its problems, or none.</summary>
public interface IPayRunProposalRule
{
    List<string> Check(PayRunProposal item);
}

public sealed record PayRunProposalLabelLengthRule(int MaxLength) : IPayRunProposalRule
{
    public List<string> Check(PayRunProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunProposalGradeLengthRule(int MaxLength) : IPayRunProposalRule
{
    public List<string> Check(PayRunProposal item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunProposalRequiredJobCodeRule(string Message) : IPayRunProposalRule
{
    public List<string> Check(PayRunProposal item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run proposal.</summary>
public sealed record PayRunProposalValidator(List<IPayRunProposalRule> Rules)
{
    public List<string> Validate(PayRunProposal item)
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

    public void RequireValid(PayRunProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunProposal> items)
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

/// <summary>Functions of pay run proposal rules.</summary>
public static class PayRunProposalRules
{
    public static PayRunProposalValidator DefaultPayRunProposalValidator()
    {
        return new PayRunProposalValidator(
            new List<IPayRunProposalRule>
            {
                new PayRunProposalLabelLengthRule(255),
                new PayRunProposalGradeLengthRule(80),
                new PayRunProposalRequiredJobCodeRule("pay run proposal needs this field"),
            });
    }
}
