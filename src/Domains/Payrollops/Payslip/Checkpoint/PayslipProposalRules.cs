namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>A rule a payslip proposal must pass. Check returns its problems, or none.</summary>
public interface IPayslipProposalRule
{
    List<string> Check(PayslipProposal item);
}

public sealed record PayslipProposalTitleLengthRule(int MaxLength) : IPayslipProposalRule
{
    public List<string> Check(PayslipProposal item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProposalMinimumRangeRule(double Min, double Max) : IPayslipProposalRule
{
    public List<string> Check(PayslipProposal item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayslipProposalSkillLengthRule(int MaxLength) : IPayslipProposalRule
{
    public List<string> Check(PayslipProposal item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipProposalRequiredCodeRule(string Message) : IPayslipProposalRule
{
    public List<string> Check(PayslipProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip proposal.</summary>
public sealed record PayslipProposalValidator(List<IPayslipProposalRule> Rules)
{
    public List<string> Validate(PayslipProposal item)
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

    public void RequireValid(PayslipProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipProposal> items)
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

/// <summary>Functions of payslip proposal rules.</summary>
public static class PayslipProposalRules
{
    public static PayslipProposalValidator DefaultPayslipProposalValidator()
    {
        return new PayslipProposalValidator(
            new List<IPayslipProposalRule>
            {
                new PayslipProposalTitleLengthRule(32),
                new PayslipProposalMinimumRangeRule(6.8, 1042.4),
                new PayslipProposalSkillLengthRule(120),
                new PayslipProposalRequiredCodeRule("payslip proposal needs an entry"),
            });
    }
}
