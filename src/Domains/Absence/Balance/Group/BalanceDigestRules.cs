namespace Acme.Workforce.Absence.Balance.Group;

/// <summary>A rule a balance digest must pass. Check returns its problems, or none.</summary>
public interface IBalanceDigestRule
{
    List<string> Check(BalanceDigest item);
}

public sealed record BalanceDigestTitleLengthRule(int MaxLength) : IBalanceDigestRule
{
    public List<string> Check(BalanceDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceDigestRequiredTitleRule(string Message) : IBalanceDigestRule
{
    public List<string> Check(BalanceDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceDigestAttemptsRangeRule(int Min, int Max) : IBalanceDigestRule
{
    public List<string> Check(BalanceDigest item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a balance digest.</summary>
public sealed record BalanceDigestValidator(List<IBalanceDigestRule> Rules)
{
    public List<string> Validate(BalanceDigest item)
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

    public void RequireValid(BalanceDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceDigest> items)
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

/// <summary>Functions of balance digest rules.</summary>
public static class BalanceDigestRules
{
    public static BalanceDigestValidator DefaultBalanceDigestValidator()
    {
        return new BalanceDigestValidator(
            new List<IBalanceDigestRule>
            {
                new BalanceDigestTitleLengthRule(3600),
                new BalanceDigestRequiredTitleRule("balance digest needs a value"),
                new BalanceDigestAttemptsRangeRule(86400, 86400),
            });
    }
}
