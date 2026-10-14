namespace Acme.Workforce.Engagement.TownHall.Score;

/// <summary>A rule a adaptive town hall score must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTownHallScoreRule
{
    List<string> Check(AdaptiveTownHallScore item);
}

public sealed record AdaptiveTownHallScoreRequiredPhoneRule(string Message) : IAdaptiveTownHallScoreRule
{
    public List<string> Check(AdaptiveTownHallScore item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallScoreCodeLengthRule(int MaxLength) : IAdaptiveTownHallScoreRule
{
    public List<string> Check(AdaptiveTownHallScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallScoreRequiredCodeRule(string Message) : IAdaptiveTownHallScoreRule
{
    public List<string> Check(AdaptiveTownHallScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive town hall score.</summary>
public sealed record AdaptiveTownHallScoreValidator(List<IAdaptiveTownHallScoreRule> Rules)
{
    public List<string> Validate(AdaptiveTownHallScore item)
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

    public void RequireValid(AdaptiveTownHallScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTownHallScore> items)
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

/// <summary>Functions of adaptive town hall score rules.</summary>
public static class AdaptiveTownHallScoreRules
{
    public static AdaptiveTownHallScoreValidator DefaultAdaptiveTownHallScoreValidator()
    {
        return new AdaptiveTownHallScoreValidator(
            new List<IAdaptiveTownHallScoreRule>
            {
                new AdaptiveTownHallScoreRequiredPhoneRule("adaptive town hall score needs a value"),
                new AdaptiveTownHallScoreCodeLengthRule(64),
                new AdaptiveTownHallScoreRequiredCodeRule("adaptive town hall score needs an entry"),
            });
    }
}
