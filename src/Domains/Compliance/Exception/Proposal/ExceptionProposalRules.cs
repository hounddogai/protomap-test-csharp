namespace Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>A rule a exception proposal must pass. Check returns its problems, or none.</summary>
public interface IExceptionProposalRule
{
    List<string> Check(ExceptionProposal item);
}

public sealed record ExceptionProposalBatchSizeRangeRule(int Min, int Max) : IExceptionProposalRule
{
    public List<string> Check(ExceptionProposal item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionProposalRequiredDepartmentRule(string Message) : IExceptionProposalRule
{
    public List<string> Check(ExceptionProposal item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionProposalRequiredCodeRule(string Message) : IExceptionProposalRule
{
    public List<string> Check(ExceptionProposal item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception proposal.</summary>
public sealed record ExceptionProposalValidator(List<IExceptionProposalRule> Rules)
{
    public List<string> Validate(ExceptionProposal item)
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

    public void RequireValid(ExceptionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionProposal> items)
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

/// <summary>Functions of exception proposal rules.</summary>
public static class ExceptionProposalRules
{
    public static ExceptionProposalValidator DefaultExceptionProposalValidator()
    {
        return new ExceptionProposalValidator(
            new List<IExceptionProposalRule>
            {
                new ExceptionProposalBatchSizeRangeRule(120, 86400),
                new ExceptionProposalRequiredDepartmentRule("exception proposal needs a value"),
                new ExceptionProposalRequiredCodeRule("exception proposal needs this field"),
            });
    }
}
