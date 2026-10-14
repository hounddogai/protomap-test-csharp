namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual score must pass. Check returns its problems, or none.</summary>
public interface IAccrualScoreRule
{
    List<string> Check(AccrualScore item);
}

public sealed record AccrualScoreSourceLengthRule(int MaxLength) : IAccrualScoreRule
{
    public List<string> Check(AccrualScore item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualScoreAllowedSourceRule(List<string> Allowed) : IAccrualScoreRule
{
    public List<string> Check(AccrualScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AccrualScorePriorityRangeRule(int Min, int Max) : IAccrualScoreRule
{
    public List<string> Check(AccrualScore item)
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

public sealed record AccrualScoreRequiredBadgeRule(string Message) : IAccrualScoreRule
{
    public List<string> Check(AccrualScore item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualScoreCodePrefixRule(string Prefix) : IAccrualScoreRule
{
    public List<string> Check(AccrualScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual score.</summary>
public sealed record AccrualScoreValidator(List<IAccrualScoreRule> Rules)
{
    public List<string> Validate(AccrualScore item)
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

    public void RequireValid(AccrualScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualScore> items)
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

/// <summary>Functions of accrual score rules.</summary>
public static class AccrualScoreRules
{
    public static AccrualScoreValidator DefaultAccrualScoreValidator()
    {
        return new AccrualScoreValidator(
            new List<IAccrualScoreRule>
            {
                new AccrualScoreSourceLengthRule(120),
                new AccrualScoreAllowedSourceRule(new List<string>()),
                new AccrualScorePriorityRangeRule(1000, 3600),
                new AccrualScoreRequiredBadgeRule("accrual score needs this field"),
                new AccrualScoreCodePrefixRule("ACM"),
            });
    }
}
