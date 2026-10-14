namespace Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>A rule a assignment metric must pass. Check returns its problems, or none.</summary>
public interface IAssignmentMetricRule
{
    List<string> Check(AssignmentMetric item);
}

public sealed record AssignmentMetricCodeLengthRule(int MaxLength) : IAssignmentMetricRule
{
    public List<string> Check(AssignmentMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentMetricRequiredLabelRule(string Message) : IAssignmentMetricRule
{
    public List<string> Check(AssignmentMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentMetricSequenceRangeRule(int Min, int Max) : IAssignmentMetricRule
{
    public List<string> Check(AssignmentMetric item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a assignment metric.</summary>
public sealed record AssignmentMetricValidator(List<IAssignmentMetricRule> Rules)
{
    public List<string> Validate(AssignmentMetric item)
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

    public void RequireValid(AssignmentMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentMetric> items)
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

/// <summary>Functions of assignment metric rules.</summary>
public static class AssignmentMetricRules
{
    public static AssignmentMetricValidator DefaultAssignmentMetricValidator()
    {
        return new AssignmentMetricValidator(
            new List<IAssignmentMetricRule>
            {
                new AssignmentMetricCodeLengthRule(16),
                new AssignmentMetricRequiredLabelRule("assignment metric needs this field"),
                new AssignmentMetricSequenceRangeRule(64, 1000),
            });
    }
}
