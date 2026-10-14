namespace Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>A rule a assignment rollup must pass. Check returns its problems, or none.</summary>
public interface IAssignmentRollupRule
{
    List<string> Check(AssignmentRollup item);
}

public sealed record AssignmentRollupRequiredCodeRule(string Message) : IAssignmentRollupRule
{
    public List<string> Check(AssignmentRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRollupReviewerLengthRule(int MaxLength) : IAssignmentRollupRule
{
    public List<string> Check(AssignmentRollup item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentRollupTitleLengthRule(int MaxLength) : IAssignmentRollupRule
{
    public List<string> Check(AssignmentRollup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment rollup.</summary>
public sealed record AssignmentRollupValidator(List<IAssignmentRollupRule> Rules)
{
    public List<string> Validate(AssignmentRollup item)
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

    public void RequireValid(AssignmentRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentRollup> items)
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

/// <summary>Functions of assignment rollup rules.</summary>
public static class AssignmentRollupRules
{
    public static AssignmentRollupValidator DefaultAssignmentRollupValidator()
    {
        return new AssignmentRollupValidator(
            new List<IAssignmentRollupRule>
            {
                new AssignmentRollupRequiredCodeRule("assignment rollup needs a value"),
                new AssignmentRollupReviewerLengthRule(120),
                new AssignmentRollupTitleLengthRule(32),
            });
    }
}
