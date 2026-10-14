namespace Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>A rule a bonus pool exception must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolExceptionRule
{
    List<string> Check(BonusPoolException item);
}

public sealed record BonusPoolExceptionCodeLengthRule(int MaxLength) : IBonusPoolExceptionRule
{
    public List<string> Check(BonusPoolException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolExceptionCodePrefixRule(string Prefix) : IBonusPoolExceptionRule
{
    public List<string> Check(BonusPoolException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolExceptionAttemptsRangeRule(int Min, int Max) : IBonusPoolExceptionRule
{
    public List<string> Check(BonusPoolException item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolExceptionRateRangeRule(double Min, double Max) : IBonusPoolExceptionRule
{
    public List<string> Check(BonusPoolException item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a bonus pool exception.</summary>
public sealed record BonusPoolExceptionValidator(List<IBonusPoolExceptionRule> Rules)
{
    public List<string> Validate(BonusPoolException item)
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

    public void RequireValid(BonusPoolException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolException> items)
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

/// <summary>Functions of bonus pool exception rules.</summary>
public static class BonusPoolExceptionRules
{
    public static BonusPoolExceptionValidator DefaultBonusPoolExceptionValidator()
    {
        return new BonusPoolExceptionValidator(
            new List<IBonusPoolExceptionRule>
            {
                new BonusPoolExceptionCodeLengthRule(1000),
                new BonusPoolExceptionCodePrefixRule("ACM"),
                new BonusPoolExceptionAttemptsRangeRule(1000, 1000),
                new BonusPoolExceptionRateRangeRule(5.9, 2426.2),
            });
    }
}
