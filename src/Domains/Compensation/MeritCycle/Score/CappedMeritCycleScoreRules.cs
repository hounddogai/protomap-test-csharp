namespace Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>A rule a capped merit cycle score must pass. Check returns its problems, or none.</summary>
public interface ICappedMeritCycleScoreRule
{
    List<string> Check(CappedMeritCycleScore item);
}

public sealed record CappedMeritCycleScoreCodeLengthRule(int MaxLength) : ICappedMeritCycleScoreRule
{
    public List<string> Check(CappedMeritCycleScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleScoreStartsAtRangeRule(int Min, int Max) : ICappedMeritCycleScoreRule
{
    public List<string> Check(CappedMeritCycleScore item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedMeritCycleScoreRequiredCodeRule(string Message) : ICappedMeritCycleScoreRule
{
    public List<string> Check(CappedMeritCycleScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleScoreOwnerIdLengthRule(int MaxLength) : ICappedMeritCycleScoreRule
{
    public List<string> Check(CappedMeritCycleScore item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleScoreVersionRangeRule(int Min, int Max) : ICappedMeritCycleScoreRule
{
    public List<string> Check(CappedMeritCycleScore item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMeritCycleScoreCodePrefixRule(string Prefix) : ICappedMeritCycleScoreRule
{
    public List<string> Check(CappedMeritCycleScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped merit cycle score.</summary>
public sealed record CappedMeritCycleScoreValidator(List<ICappedMeritCycleScoreRule> Rules)
{
    public List<string> Validate(CappedMeritCycleScore item)
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

    public void RequireValid(CappedMeritCycleScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedMeritCycleScore> items)
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

/// <summary>Functions of capped merit cycle score rules.</summary>
public static class CappedMeritCycleScoreRules
{
    public static CappedMeritCycleScoreValidator DefaultCappedMeritCycleScoreValidator()
    {
        return new CappedMeritCycleScoreValidator(
            new List<ICappedMeritCycleScoreRule>
            {
                new CappedMeritCycleScoreCodeLengthRule(1000),
                new CappedMeritCycleScoreStartsAtRangeRule(32, 255),
                new CappedMeritCycleScoreRequiredCodeRule("capped merit cycle score needs this field"),
                new CappedMeritCycleScoreOwnerIdLengthRule(32),
                new CappedMeritCycleScoreVersionRangeRule(16, 64),
                new CappedMeritCycleScoreCodePrefixRule("Q"),
            });
    }
}
