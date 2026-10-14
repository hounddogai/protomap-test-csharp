namespace Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>A rule a global payslip proposal must pass. Check returns its problems, or none.</summary>
public interface IGlobalPayslipProposalRule
{
    List<string> Check(GlobalPayslipProposal item);
}

public sealed record GlobalPayslipProposalRequiredCodeRule(string Message) : IGlobalPayslipProposalRule
{
    public List<string> Check(GlobalPayslipProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayslipProposalCodeLengthRule(int MaxLength) : IGlobalPayslipProposalRule
{
    public List<string> Check(GlobalPayslipProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPayslipProposalGraceDaysRangeRule(int Min, int Max) : IGlobalPayslipProposalRule
{
    public List<string> Check(GlobalPayslipProposal item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global payslip proposal.</summary>
public sealed record GlobalPayslipProposalValidator(List<IGlobalPayslipProposalRule> Rules)
{
    public List<string> Validate(GlobalPayslipProposal item)
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

    public void RequireValid(GlobalPayslipProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPayslipProposal> items)
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

/// <summary>Functions of global payslip proposal rules.</summary>
public static class GlobalPayslipProposalRules
{
    public static GlobalPayslipProposalValidator DefaultGlobalPayslipProposalValidator()
    {
        return new GlobalPayslipProposalValidator(
            new List<IGlobalPayslipProposalRule>
            {
                new GlobalPayslipProposalRequiredCodeRule("global payslip proposal needs this field"),
                new GlobalPayslipProposalCodeLengthRule(3600),
                new GlobalPayslipProposalGraceDaysRangeRule(64, 86400),
            });
    }
}
