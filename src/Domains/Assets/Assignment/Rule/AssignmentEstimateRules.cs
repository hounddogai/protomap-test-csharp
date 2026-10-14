namespace Acme.Workforce.Assets.Assignment.Rule;

/// <summary>A rule a assignment estimate must pass. Check returns its problems, or none.</summary>
public interface IAssignmentEstimateRule
{
    List<string> Check(AssignmentEstimate item);
}

public sealed record AssignmentEstimateRequiredCostCenterRule(string Message) : IAssignmentEstimateRule
{
    public List<string> Check(AssignmentEstimate item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentEstimateCodeLengthRule(int MaxLength) : IAssignmentEstimateRule
{
    public List<string> Check(AssignmentEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentEstimateQuantityRangeRule(int Min, int Max) : IAssignmentEstimateRule
{
    public List<string> Check(AssignmentEstimate item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment estimate.</summary>
public sealed record AssignmentEstimateValidator(List<IAssignmentEstimateRule> Rules)
{
    public List<string> Validate(AssignmentEstimate item)
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

    public void RequireValid(AssignmentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentEstimate> items)
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

/// <summary>Functions of assignment estimate rules.</summary>
public static class AssignmentEstimateRules
{
    public static AssignmentEstimateValidator DefaultAssignmentEstimateValidator()
    {
        return new AssignmentEstimateValidator(
            new List<IAssignmentEstimateRule>
            {
                new AssignmentEstimateRequiredCostCenterRule("assignment estimate needs a value"),
                new AssignmentEstimateCodeLengthRule(16),
                new AssignmentEstimateQuantityRangeRule(80, 120),
            });
    }
}
