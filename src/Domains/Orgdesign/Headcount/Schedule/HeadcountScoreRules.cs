namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>A rule a headcount score must pass. Check returns its problems, or none.</summary>
public interface IHeadcountScoreRule
{
    List<string> Check(HeadcountScore item);
}

public sealed record HeadcountScoreThresholdRangeRule(double Min, double Max) : IHeadcountScoreRule
{
    public List<string> Check(HeadcountScore item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountScoreMarginRangeRule(double Min, double Max) : IHeadcountScoreRule
{
    public List<string> Check(HeadcountScore item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountScoreTitleLengthRule(int MaxLength) : IHeadcountScoreRule
{
    public List<string> Check(HeadcountScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount score.</summary>
public sealed record HeadcountScoreValidator(List<IHeadcountScoreRule> Rules)
{
    public List<string> Validate(HeadcountScore item)
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

    public void RequireValid(HeadcountScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountScore> items)
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

/// <summary>Functions of headcount score rules.</summary>
public static class HeadcountScoreRules
{
    public static HeadcountScoreValidator DefaultHeadcountScoreValidator()
    {
        return new HeadcountScoreValidator(
            new List<IHeadcountScoreRule>
            {
                new HeadcountScoreThresholdRangeRule(7.8, 4289.4),
                new HeadcountScoreMarginRangeRule(6.7, 1286.8),
                new HeadcountScoreTitleLengthRule(1000),
            });
    }
}
