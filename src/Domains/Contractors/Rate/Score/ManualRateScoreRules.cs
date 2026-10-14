namespace Acme.Workforce.Contractors.Rate.Score;

/// <summary>A rule a manual rate score must pass. Check returns its problems, or none.</summary>
public interface IManualRateScoreRule
{
    List<string> Check(ManualRateScore item);
}

public sealed record ManualRateScoreNotesLengthRule(int MaxLength) : IManualRateScoreRule
{
    public List<string> Check(ManualRateScore item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateScoreNameLengthRule(int MaxLength) : IManualRateScoreRule
{
    public List<string> Check(ManualRateScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateScoreCodeLengthRule(int MaxLength) : IManualRateScoreRule
{
    public List<string> Check(ManualRateScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual rate score.</summary>
public sealed record ManualRateScoreValidator(List<IManualRateScoreRule> Rules)
{
    public List<string> Validate(ManualRateScore item)
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

    public void RequireValid(ManualRateScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRateScore> items)
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

/// <summary>Functions of manual rate score rules.</summary>
public static class ManualRateScoreRules
{
    public static ManualRateScoreValidator DefaultManualRateScoreValidator()
    {
        return new ManualRateScoreValidator(
            new List<IManualRateScoreRule>
            {
                new ManualRateScoreNotesLengthRule(80),
                new ManualRateScoreNameLengthRule(86400),
                new ManualRateScoreCodeLengthRule(3600),
            });
    }
}
