namespace Acme.Workforce.Absence.Balance.Tier;

/// <summary>A rule a balance cycle must pass. Check returns its problems, or none.</summary>
public interface IBalanceCycleRule
{
    List<string> Check(BalanceCycle item);
}

public sealed record BalanceCycleRequiredTitleRule(string Message) : IBalanceCycleRule
{
    public List<string> Check(BalanceCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceCycleRequiredCodeRule(string Message) : IBalanceCycleRule
{
    public List<string> Check(BalanceCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceCycleTitleLengthRule(int MaxLength) : IBalanceCycleRule
{
    public List<string> Check(BalanceCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceCycleCodeLengthRule(int MaxLength) : IBalanceCycleRule
{
    public List<string> Check(BalanceCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance cycle.</summary>
public sealed record BalanceCycleValidator(List<IBalanceCycleRule> Rules)
{
    public List<string> Validate(BalanceCycle item)
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

    public void RequireValid(BalanceCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceCycle> items)
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

/// <summary>Functions of balance cycle rules.</summary>
public static class BalanceCycleRules
{
    public static BalanceCycleValidator DefaultBalanceCycleValidator()
    {
        return new BalanceCycleValidator(
            new List<IBalanceCycleRule>
            {
                new BalanceCycleRequiredTitleRule("balance cycle needs this field"),
                new BalanceCycleRequiredCodeRule("balance cycle needs this field"),
                new BalanceCycleTitleLengthRule(255),
                new BalanceCycleCodeLengthRule(64),
            });
    }
}
