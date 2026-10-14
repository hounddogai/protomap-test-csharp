namespace Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>A rule a strict accrual tier must pass. Check returns its problems, or none.</summary>
public interface IStrictAccrualTierRule
{
    List<string> Check(StrictAccrualTier item);
}

public sealed record StrictAccrualTierCodeLengthRule(int MaxLength) : IStrictAccrualTierRule
{
    public List<string> Check(StrictAccrualTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualTierRequiredTitleRule(string Message) : IStrictAccrualTierRule
{
    public List<string> Check(StrictAccrualTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualTierRequiredDescriptionRule(string Message) : IStrictAccrualTierRule
{
    public List<string> Check(StrictAccrualTier item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict accrual tier.</summary>
public sealed record StrictAccrualTierValidator(List<IStrictAccrualTierRule> Rules)
{
    public List<string> Validate(StrictAccrualTier item)
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

    public void RequireValid(StrictAccrualTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccrualTier> items)
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

/// <summary>Functions of strict accrual tier rules.</summary>
public static class StrictAccrualTierRules
{
    public static StrictAccrualTierValidator DefaultStrictAccrualTierValidator()
    {
        return new StrictAccrualTierValidator(
            new List<IStrictAccrualTierRule>
            {
                new StrictAccrualTierCodeLengthRule(32),
                new StrictAccrualTierRequiredTitleRule("strict accrual tier needs an entry"),
                new StrictAccrualTierRequiredDescriptionRule("strict accrual tier needs an entry"),
            });
    }
}
