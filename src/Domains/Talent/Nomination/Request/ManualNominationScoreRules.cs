namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual nomination score must pass. Check returns its problems, or none.</summary>
public interface IManualNominationScoreRule
{
    List<string> Check(ManualNominationScore item);
}

public sealed record ManualNominationScoreScoreRangeRule(double Min, double Max) : IManualNominationScoreRule
{
    public List<string> Check(ManualNominationScore item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualNominationScoreCeilingRangeRule(double Min, double Max) : IManualNominationScoreRule
{
    public List<string> Check(ManualNominationScore item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualNominationScoreAllowedTitleRule(List<string> Allowed) : IManualNominationScoreRule
{
    public List<string> Check(ManualNominationScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ManualNominationScoreRequiredTitleRule(string Message) : IManualNominationScoreRule
{
    public List<string> Check(ManualNominationScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualNominationScoreRatingRangeRule(double Min, double Max) : IManualNominationScoreRule
{
    public List<string> Check(ManualNominationScore item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual nomination score.</summary>
public sealed record ManualNominationScoreValidator(List<IManualNominationScoreRule> Rules)
{
    public List<string> Validate(ManualNominationScore item)
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

    public void RequireValid(ManualNominationScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualNominationScore> items)
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

/// <summary>Functions of manual nomination score rules.</summary>
public static class ManualNominationScoreRules
{
    public static ManualNominationScoreValidator DefaultManualNominationScoreValidator()
    {
        return new ManualNominationScoreValidator(
            new List<IManualNominationScoreRule>
            {
                new ManualNominationScoreScoreRangeRule(0.2, 2977.2),
                new ManualNominationScoreCeilingRangeRule(7.9, 2029.2),
                new ManualNominationScoreAllowedTitleRule(new List<string> { "legacy", "weighted" }),
                new ManualNominationScoreRequiredTitleRule("manual nomination score needs a value"),
                new ManualNominationScoreRatingRangeRule(8.8, 1877.3),
            });
    }
}
