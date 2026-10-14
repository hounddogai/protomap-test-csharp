namespace Acme.Workforce.Talent.Nomination.Score;

/// <summary>A rule a strict nomination score must pass. Check returns its problems, or none.</summary>
public interface IStrictNominationScoreRule
{
    List<string> Check(StrictNominationScore item);
}

public sealed record StrictNominationScorePinnedGuardRule(double Limit) : IStrictNominationScoreRule
{
    public List<string> Check(StrictNominationScore item)
    {
        if (item.Pinned && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationScoreReviewerLengthRule(int MaxLength) : IStrictNominationScoreRule
{
    public List<string> Check(StrictNominationScore item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationScoreBudgetRangeRule(double Min, double Max) : IStrictNominationScoreRule
{
    public List<string> Check(StrictNominationScore item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationScoreCodePrefixRule(string Prefix) : IStrictNominationScoreRule
{
    public List<string> Check(StrictNominationScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationScoreRequiredCodeRule(string Message) : IStrictNominationScoreRule
{
    public List<string> Check(StrictNominationScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationScoreDurationMinutesRangeRule(int Min, int Max) : IStrictNominationScoreRule
{
    public List<string> Check(StrictNominationScore item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict nomination score.</summary>
public sealed record StrictNominationScoreValidator(List<IStrictNominationScoreRule> Rules)
{
    public List<string> Validate(StrictNominationScore item)
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

    public void RequireValid(StrictNominationScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictNominationScore> items)
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

/// <summary>Functions of strict nomination score rules.</summary>
public static class StrictNominationScoreRules
{
    public static StrictNominationScoreValidator DefaultStrictNominationScoreValidator()
    {
        return new StrictNominationScoreValidator(
            new List<IStrictNominationScoreRule>
            {
                new StrictNominationScorePinnedGuardRule(5.7),
                new StrictNominationScoreReviewerLengthRule(255),
                new StrictNominationScoreBudgetRangeRule(0.6, 2352.4),
                new StrictNominationScoreCodePrefixRule("REF"),
                new StrictNominationScoreRequiredCodeRule("strict nomination score needs this field"),
                new StrictNominationScoreDurationMinutesRangeRule(16, 80),
            });
    }
}
