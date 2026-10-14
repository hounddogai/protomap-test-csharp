namespace Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>A rule a bonus pool group must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolGroupRule
{
    List<string> Check(BonusPoolGroup item);
}

public sealed record BonusPoolGroupCodeLengthRule(int MaxLength) : IBonusPoolGroupRule
{
    public List<string> Check(BonusPoolGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolGroupBudgetRangeRule(double Min, double Max) : IBonusPoolGroupRule
{
    public List<string> Check(BonusPoolGroup item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolGroupRequiredLabelRule(string Message) : IBonusPoolGroupRule
{
    public List<string> Check(BonusPoolGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool group.</summary>
public sealed record BonusPoolGroupValidator(List<IBonusPoolGroupRule> Rules)
{
    public List<string> Validate(BonusPoolGroup item)
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

    public void RequireValid(BonusPoolGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolGroup> items)
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

/// <summary>Functions of bonus pool group rules.</summary>
public static class BonusPoolGroupRules
{
    public static BonusPoolGroupValidator DefaultBonusPoolGroupValidator()
    {
        return new BonusPoolGroupValidator(
            new List<IBonusPoolGroupRule>
            {
                new BonusPoolGroupCodeLengthRule(3600),
                new BonusPoolGroupBudgetRangeRule(9.2, 1489.9),
                new BonusPoolGroupRequiredLabelRule("bonus pool group needs an entry"),
            });
    }
}
