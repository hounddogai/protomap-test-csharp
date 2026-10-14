namespace Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>A rule a adjustment score must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentScoreRule
{
    List<string> Check(AdjustmentScore item);
}

public sealed record AdjustmentScoreRequiredDepartmentRule(string Message) : IAdjustmentScoreRule
{
    public List<string> Check(AdjustmentScore item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScoreTitleLengthRule(int MaxLength) : IAdjustmentScoreRule
{
    public List<string> Check(AdjustmentScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScoreRequiredTitleRule(string Message) : IAdjustmentScoreRule
{
    public List<string> Check(AdjustmentScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScoreSourceLengthRule(int MaxLength) : IAdjustmentScoreRule
{
    public List<string> Check(AdjustmentScore item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScoreRequiredSourceRule(string Message) : IAdjustmentScoreRule
{
    public List<string> Check(AdjustmentScore item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentScoreCodeLengthRule(int MaxLength) : IAdjustmentScoreRule
{
    public List<string> Check(AdjustmentScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment score.</summary>
public sealed record AdjustmentScoreValidator(List<IAdjustmentScoreRule> Rules)
{
    public List<string> Validate(AdjustmentScore item)
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

    public void RequireValid(AdjustmentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentScore> items)
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

/// <summary>Functions of adjustment score rules.</summary>
public static class AdjustmentScoreRules
{
    public static AdjustmentScoreValidator DefaultAdjustmentScoreValidator()
    {
        return new AdjustmentScoreValidator(
            new List<IAdjustmentScoreRule>
            {
                new AdjustmentScoreRequiredDepartmentRule("adjustment score needs a value"),
                new AdjustmentScoreTitleLengthRule(1000),
                new AdjustmentScoreRequiredTitleRule("adjustment score needs this field"),
                new AdjustmentScoreSourceLengthRule(1000),
                new AdjustmentScoreRequiredSourceRule("adjustment score needs this field"),
                new AdjustmentScoreCodeLengthRule(255),
            });
    }
}
