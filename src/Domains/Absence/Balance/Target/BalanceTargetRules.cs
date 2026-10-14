namespace Acme.Workforce.Absence.Balance.Target;

/// <summary>A rule a balance target must pass. Check returns its problems, or none.</summary>
public interface IBalanceTargetRule
{
    List<string> Check(BalanceTarget item);
}

public sealed record BalanceTargetQuantityRangeRule(int Min, int Max) : IBalanceTargetRule
{
    public List<string> Check(BalanceTarget item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTargetCodeLengthRule(int MaxLength) : IBalanceTargetRule
{
    public List<string> Check(BalanceTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTargetRequiredTitleRule(string Message) : IBalanceTargetRule
{
    public List<string> Check(BalanceTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance target.</summary>
public sealed record BalanceTargetValidator(List<IBalanceTargetRule> Rules)
{
    public List<string> Validate(BalanceTarget item)
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

    public void RequireValid(BalanceTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceTarget> items)
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

/// <summary>Functions of balance target rules.</summary>
public static class BalanceTargetRules
{
    public static BalanceTargetValidator DefaultBalanceTargetValidator()
    {
        return new BalanceTargetValidator(
            new List<IBalanceTargetRule>
            {
                new BalanceTargetQuantityRangeRule(32, 3600),
                new BalanceTargetCodeLengthRule(3600),
                new BalanceTargetRequiredTitleRule("balance target needs a value"),
            });
    }
}
