namespace Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>A rule a assignment budget must pass. Check returns its problems, or none.</summary>
public interface IAssignmentBudgetRule
{
    List<string> Check(AssignmentBudget item);
}

public sealed record AssignmentBudgetArchivedGuardRule(double Limit) : IAssignmentBudgetRule
{
    public List<string> Check(AssignmentBudget item)
    {
        if (item.Archived && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBudgetSourceLengthRule(int MaxLength) : IAssignmentBudgetRule
{
    public List<string> Check(AssignmentBudget item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBudgetLabelLengthRule(int MaxLength) : IAssignmentBudgetRule
{
    public List<string> Check(AssignmentBudget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBudgetRequiredSourceRule(string Message) : IAssignmentBudgetRule
{
    public List<string> Check(AssignmentBudget item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment budget.</summary>
public sealed record AssignmentBudgetValidator(List<IAssignmentBudgetRule> Rules)
{
    public List<string> Validate(AssignmentBudget item)
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

    public void RequireValid(AssignmentBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentBudget> items)
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

/// <summary>Functions of assignment budget rules.</summary>
public static class AssignmentBudgetRules
{
    public static AssignmentBudgetValidator DefaultAssignmentBudgetValidator()
    {
        return new AssignmentBudgetValidator(
            new List<IAssignmentBudgetRule>
            {
                new AssignmentBudgetArchivedGuardRule(4.7),
                new AssignmentBudgetSourceLengthRule(86400),
                new AssignmentBudgetLabelLengthRule(255),
                new AssignmentBudgetRequiredSourceRule("assignment budget needs an entry"),
            });
    }
}
