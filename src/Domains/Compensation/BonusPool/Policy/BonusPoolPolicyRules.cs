namespace Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>A rule a bonus pool policy must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolPolicyRule
{
    List<string> Check(BonusPoolPolicy item);
}

public sealed record BonusPoolPolicyLeaveDaysRangeRule(int Min, int Max) : IBonusPoolPolicyRule
{
    public List<string> Check(BonusPoolPolicy item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolPolicyRequiredCodeRule(string Message) : IBonusPoolPolicyRule
{
    public List<string> Check(BonusPoolPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolPolicyRequiredNameRule(string Message) : IBonusPoolPolicyRule
{
    public List<string> Check(BonusPoolPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool policy.</summary>
public sealed record BonusPoolPolicyValidator(List<IBonusPoolPolicyRule> Rules)
{
    public List<string> Validate(BonusPoolPolicy item)
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

    public void RequireValid(BonusPoolPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolPolicy> items)
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

/// <summary>Functions of bonus pool policy rules.</summary>
public static class BonusPoolPolicyRules
{
    public static BonusPoolPolicyValidator DefaultBonusPoolPolicyValidator()
    {
        return new BonusPoolPolicyValidator(
            new List<IBonusPoolPolicyRule>
            {
                new BonusPoolPolicyLeaveDaysRangeRule(120, 1000),
                new BonusPoolPolicyRequiredCodeRule("bonus pool policy needs an entry"),
                new BonusPoolPolicyRequiredNameRule("bonus pool policy needs a value"),
            });
    }
}
