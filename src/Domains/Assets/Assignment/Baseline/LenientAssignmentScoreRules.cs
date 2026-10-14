namespace Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>A rule a lenient assignment score must pass. Check returns its problems, or none.</summary>
public interface ILenientAssignmentScoreRule
{
    List<string> Check(LenientAssignmentScore item);
}

public sealed record LenientAssignmentScoreCategoryLengthRule(int MaxLength) : ILenientAssignmentScoreRule
{
    public List<string> Check(LenientAssignmentScore item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssignmentScoreCostRangeRule(double Min, double Max) : ILenientAssignmentScoreRule
{
    public List<string> Check(LenientAssignmentScore item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientAssignmentScoreCodeLengthRule(int MaxLength) : ILenientAssignmentScoreRule
{
    public List<string> Check(LenientAssignmentScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssignmentScoreLabelLengthRule(int MaxLength) : ILenientAssignmentScoreRule
{
    public List<string> Check(LenientAssignmentScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssignmentScoreRequiredLabelRule(string Message) : ILenientAssignmentScoreRule
{
    public List<string> Check(LenientAssignmentScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient assignment score.</summary>
public sealed record LenientAssignmentScoreValidator(List<ILenientAssignmentScoreRule> Rules)
{
    public List<string> Validate(LenientAssignmentScore item)
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

    public void RequireValid(LenientAssignmentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAssignmentScore> items)
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

/// <summary>Functions of lenient assignment score rules.</summary>
public static class LenientAssignmentScoreRules
{
    public static LenientAssignmentScoreValidator DefaultLenientAssignmentScoreValidator()
    {
        return new LenientAssignmentScoreValidator(
            new List<ILenientAssignmentScoreRule>
            {
                new LenientAssignmentScoreCategoryLengthRule(255),
                new LenientAssignmentScoreCostRangeRule(7.5, 2890.2),
                new LenientAssignmentScoreCodeLengthRule(255),
                new LenientAssignmentScoreLabelLengthRule(3600),
                new LenientAssignmentScoreRequiredLabelRule("lenient assignment score needs an entry"),
            });
    }
}
