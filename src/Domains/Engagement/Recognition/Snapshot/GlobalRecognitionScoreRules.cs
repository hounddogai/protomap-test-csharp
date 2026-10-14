namespace Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>A rule a global recognition score must pass. Check returns its problems, or none.</summary>
public interface IGlobalRecognitionScoreRule
{
    List<string> Check(GlobalRecognitionScore item);
}

public sealed record GlobalRecognitionScoreVisibleGuardRule(double Limit) : IGlobalRecognitionScoreRule
{
    public List<string> Check(GlobalRecognitionScore item)
    {
        if (item.Visible && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRecognitionScoreRequiredTimezoneRule(string Message) : IGlobalRecognitionScoreRule
{
    public List<string> Check(GlobalRecognitionScore item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalRecognitionScoreLimitRangeRule(int Min, int Max) : IGlobalRecognitionScoreRule
{
    public List<string> Check(GlobalRecognitionScore item)
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

public sealed record GlobalRecognitionScoreRequiredCodeRule(string Message) : IGlobalRecognitionScoreRule
{
    public List<string> Check(GlobalRecognitionScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalRecognitionScoreTimezoneLengthRule(int MaxLength) : IGlobalRecognitionScoreRule
{
    public List<string> Check(GlobalRecognitionScore item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRecognitionScoreCodePrefixRule(string Prefix) : IGlobalRecognitionScoreRule
{
    public List<string> Check(GlobalRecognitionScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global recognition score.</summary>
public sealed record GlobalRecognitionScoreValidator(List<IGlobalRecognitionScoreRule> Rules)
{
    public List<string> Validate(GlobalRecognitionScore item)
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

    public void RequireValid(GlobalRecognitionScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalRecognitionScore> items)
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

/// <summary>Functions of global recognition score rules.</summary>
public static class GlobalRecognitionScoreRules
{
    public static GlobalRecognitionScoreValidator DefaultGlobalRecognitionScoreValidator()
    {
        return new GlobalRecognitionScoreValidator(
            new List<IGlobalRecognitionScoreRule>
            {
                new GlobalRecognitionScoreVisibleGuardRule(6.6),
                new GlobalRecognitionScoreRequiredTimezoneRule("global recognition score needs this field"),
                new GlobalRecognitionScoreLimitRangeRule(16, 3600),
                new GlobalRecognitionScoreRequiredCodeRule("global recognition score needs an entry"),
                new GlobalRecognitionScoreTimezoneLengthRule(64),
                new GlobalRecognitionScoreCodePrefixRule("X"),
            });
    }
}
