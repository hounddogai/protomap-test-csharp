namespace Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>A rule a manual career path digest must pass. Check returns its problems, or none.</summary>
public interface IManualCareerPathDigestRule
{
    List<string> Check(ManualCareerPathDigest item);
}

public sealed record ManualCareerPathDigestScoreRangeRule(double Min, double Max) : IManualCareerPathDigestRule
{
    public List<string> Check(ManualCareerPathDigest item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualCareerPathDigestCodeLengthRule(int MaxLength) : IManualCareerPathDigestRule
{
    public List<string> Check(ManualCareerPathDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathDigestRequiredLocaleRule(string Message) : IManualCareerPathDigestRule
{
    public List<string> Check(ManualCareerPathDigest item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathDigestSourceLengthRule(int MaxLength) : IManualCareerPathDigestRule
{
    public List<string> Check(ManualCareerPathDigest item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathDigestRequiredLabelRule(string Message) : IManualCareerPathDigestRule
{
    public List<string> Check(ManualCareerPathDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualCareerPathDigestPriorityRangeRule(int Min, int Max) : IManualCareerPathDigestRule
{
    public List<string> Check(ManualCareerPathDigest item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual career path digest.</summary>
public sealed record ManualCareerPathDigestValidator(List<IManualCareerPathDigestRule> Rules)
{
    public List<string> Validate(ManualCareerPathDigest item)
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

    public void RequireValid(ManualCareerPathDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualCareerPathDigest> items)
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

/// <summary>Functions of manual career path digest rules.</summary>
public static class ManualCareerPathDigestRules
{
    public static ManualCareerPathDigestValidator DefaultManualCareerPathDigestValidator()
    {
        return new ManualCareerPathDigestValidator(
            new List<IManualCareerPathDigestRule>
            {
                new ManualCareerPathDigestScoreRangeRule(1.7, 1517.2),
                new ManualCareerPathDigestCodeLengthRule(3600),
                new ManualCareerPathDigestRequiredLocaleRule("manual career path digest needs a value"),
                new ManualCareerPathDigestSourceLengthRule(86400),
                new ManualCareerPathDigestRequiredLabelRule("manual career path digest needs this field"),
                new ManualCareerPathDigestPriorityRangeRule(3600, 3600),
            });
    }
}
