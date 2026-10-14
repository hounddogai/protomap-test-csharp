namespace Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>A rule a scheduled retro pay score must pass. Check returns its problems, or none.</summary>
public interface IScheduledRetroPayScoreRule
{
    List<string> Check(ScheduledRetroPayScore item);
}

public sealed record ScheduledRetroPayScoreRequiredTitleRule(string Message) : IScheduledRetroPayScoreRule
{
    public List<string> Check(ScheduledRetroPayScore item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayScoreCodePrefixRule(string Prefix) : IScheduledRetroPayScoreRule
{
    public List<string> Check(ScheduledRetroPayScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRetroPayScoreCodeLengthRule(int MaxLength) : IScheduledRetroPayScoreRule
{
    public List<string> Check(ScheduledRetroPayScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled retro pay score.</summary>
public sealed record ScheduledRetroPayScoreValidator(List<IScheduledRetroPayScoreRule> Rules)
{
    public List<string> Validate(ScheduledRetroPayScore item)
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

    public void RequireValid(ScheduledRetroPayScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRetroPayScore> items)
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

/// <summary>Functions of scheduled retro pay score rules.</summary>
public static class ScheduledRetroPayScoreRules
{
    public static ScheduledRetroPayScoreValidator DefaultScheduledRetroPayScoreValidator()
    {
        return new ScheduledRetroPayScoreValidator(
            new List<IScheduledRetroPayScoreRule>
            {
                new ScheduledRetroPayScoreRequiredTitleRule("scheduled retro pay score needs an entry"),
                new ScheduledRetroPayScoreCodePrefixRule("OPS"),
                new ScheduledRetroPayScoreCodeLengthRule(3600),
            });
    }
}
