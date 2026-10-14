namespace Acme.Workforce.Assets.Assignment.Allocation;

/// <summary>A rule a assignment proposal must pass. Check returns its problems, or none.</summary>
public interface IAssignmentProposalRule
{
    List<string> Check(AssignmentProposal item);
}

public sealed record AssignmentProposalRequiredReviewerRule(string Message) : IAssignmentProposalRule
{
    public List<string> Check(AssignmentProposal item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProposalTitleLengthRule(int MaxLength) : IAssignmentProposalRule
{
    public List<string> Check(AssignmentProposal item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProposalReviewerLengthRule(int MaxLength) : IAssignmentProposalRule
{
    public List<string> Check(AssignmentProposal item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProposalEmployeeIdLengthRule(int MaxLength) : IAssignmentProposalRule
{
    public List<string> Check(AssignmentProposal item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProposalCodeLengthRule(int MaxLength) : IAssignmentProposalRule
{
    public List<string> Check(AssignmentProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProposalExpiresAtRangeRule(int Min, int Max) : IAssignmentProposalRule
{
    public List<string> Check(AssignmentProposal item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a assignment proposal.</summary>
public sealed record AssignmentProposalValidator(List<IAssignmentProposalRule> Rules)
{
    public List<string> Validate(AssignmentProposal item)
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

    public void RequireValid(AssignmentProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentProposal> items)
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

/// <summary>Functions of assignment proposal rules.</summary>
public static class AssignmentProposalRules
{
    public static AssignmentProposalValidator DefaultAssignmentProposalValidator()
    {
        return new AssignmentProposalValidator(
            new List<IAssignmentProposalRule>
            {
                new AssignmentProposalRequiredReviewerRule("assignment proposal needs an entry"),
                new AssignmentProposalTitleLengthRule(16),
                new AssignmentProposalReviewerLengthRule(32),
                new AssignmentProposalEmployeeIdLengthRule(3600),
                new AssignmentProposalCodeLengthRule(16),
                new AssignmentProposalExpiresAtRangeRule(255, 86400),
            });
    }
}
