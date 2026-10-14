namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>A rule a cost model assignment must pass. Check returns its problems, or none.</summary>
public interface ICostModelAssignmentRule
{
    List<string> Check(CostModelAssignment item);
}

public sealed record CostModelAssignmentRequiredCodeRule(string Message) : ICostModelAssignmentRule
{
    public List<string> Check(CostModelAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelAssignmentCodeLengthRule(int MaxLength) : ICostModelAssignmentRule
{
    public List<string> Check(CostModelAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAssignmentRequiredReviewerRule(string Message) : ICostModelAssignmentRule
{
    public List<string> Check(CostModelAssignment item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model assignment.</summary>
public sealed record CostModelAssignmentValidator(List<ICostModelAssignmentRule> Rules)
{
    public List<string> Validate(CostModelAssignment item)
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

    public void RequireValid(CostModelAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelAssignment> items)
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

/// <summary>Functions of cost model assignment rules.</summary>
public static class CostModelAssignmentRules
{
    public static CostModelAssignmentValidator DefaultCostModelAssignmentValidator()
    {
        return new CostModelAssignmentValidator(
            new List<ICostModelAssignmentRule>
            {
                new CostModelAssignmentRequiredCodeRule("cost model assignment needs a value"),
                new CostModelAssignmentCodeLengthRule(32),
                new CostModelAssignmentRequiredReviewerRule("cost model assignment needs this field"),
            });
    }
}
