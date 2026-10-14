namespace Acme.Workforce.Talent.Pool.Draft;

/// <summary>A rule a pool draft must pass. Check returns its problems, or none.</summary>
public interface IPoolDraftRule
{
    List<string> Check(PoolDraft item);
}

public sealed record PoolDraftAmountRangeRule(double Min, double Max) : IPoolDraftRule
{
    public List<string> Check(PoolDraft item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolDraftLocaleLengthRule(int MaxLength) : IPoolDraftRule
{
    public List<string> Check(PoolDraft item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolDraftRequiredGradeRule(string Message) : IPoolDraftRule
{
    public List<string> Check(PoolDraft item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolDraftCodeLengthRule(int MaxLength) : IPoolDraftRule
{
    public List<string> Check(PoolDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolDraftExpiresAtRangeRule(int Min, int Max) : IPoolDraftRule
{
    public List<string> Check(PoolDraft item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolDraftRequiredTitleRule(string Message) : IPoolDraftRule
{
    public List<string> Check(PoolDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool draft.</summary>
public sealed record PoolDraftValidator(List<IPoolDraftRule> Rules)
{
    public List<string> Validate(PoolDraft item)
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

    public void RequireValid(PoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolDraft> items)
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

/// <summary>Functions of pool draft rules.</summary>
public static class PoolDraftRules
{
    public static PoolDraftValidator DefaultPoolDraftValidator()
    {
        return new PoolDraftValidator(
            new List<IPoolDraftRule>
            {
                new PoolDraftAmountRangeRule(0.2, 4658.3),
                new PoolDraftLocaleLengthRule(255),
                new PoolDraftRequiredGradeRule("pool draft needs a value"),
                new PoolDraftCodeLengthRule(16),
                new PoolDraftExpiresAtRangeRule(255, 3600),
                new PoolDraftRequiredTitleRule("pool draft needs this field"),
            });
    }
}
