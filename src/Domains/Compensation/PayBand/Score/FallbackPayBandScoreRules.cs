namespace Acme.Workforce.Compensation.PayBand.Score;

/// <summary>A rule a fallback pay band score must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayBandScoreRule
{
    List<string> Check(FallbackPayBandScore item);
}

public sealed record FallbackPayBandScoreNameLengthRule(int MaxLength) : IFallbackPayBandScoreRule
{
    public List<string> Check(FallbackPayBandScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandScoreNotesLengthRule(int MaxLength) : IFallbackPayBandScoreRule
{
    public List<string> Check(FallbackPayBandScore item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandScoreLimitRangeRule(int Min, int Max) : IFallbackPayBandScoreRule
{
    public List<string> Check(FallbackPayBandScore item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPayBandScoreCodeLengthRule(int MaxLength) : IFallbackPayBandScoreRule
{
    public List<string> Check(FallbackPayBandScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pay band score.</summary>
public sealed record FallbackPayBandScoreValidator(List<IFallbackPayBandScoreRule> Rules)
{
    public List<string> Validate(FallbackPayBandScore item)
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

    public void RequireValid(FallbackPayBandScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayBandScore> items)
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

/// <summary>Functions of fallback pay band score rules.</summary>
public static class FallbackPayBandScoreRules
{
    public static FallbackPayBandScoreValidator DefaultFallbackPayBandScoreValidator()
    {
        return new FallbackPayBandScoreValidator(
            new List<IFallbackPayBandScoreRule>
            {
                new FallbackPayBandScoreNameLengthRule(255),
                new FallbackPayBandScoreNotesLengthRule(86400),
                new FallbackPayBandScoreLimitRangeRule(32, 64),
                new FallbackPayBandScoreCodeLengthRule(3600),
            });
    }
}
