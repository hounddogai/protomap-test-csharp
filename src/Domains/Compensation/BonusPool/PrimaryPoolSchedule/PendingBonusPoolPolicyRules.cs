namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>A rule a pending bonus pool policy must pass. Check returns its problems, or none.</summary>
public interface IPendingBonusPoolPolicyRule
{
    List<string> Check(PendingBonusPoolPolicy item);
}

public sealed record PendingBonusPoolPolicyUnitLengthRule(int MaxLength) : IPendingBonusPoolPolicyRule
{
    public List<string> Check(PendingBonusPoolPolicy item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolPolicyCodeLengthRule(int MaxLength) : IPendingBonusPoolPolicyRule
{
    public List<string> Check(PendingBonusPoolPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolPolicyTitleLengthRule(int MaxLength) : IPendingBonusPoolPolicyRule
{
    public List<string> Check(PendingBonusPoolPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolPolicySalaryBandLengthRule(int MaxLength) : IPendingBonusPoolPolicyRule
{
    public List<string> Check(PendingBonusPoolPolicy item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBonusPoolPolicyRequiredUnitRule(string Message) : IPendingBonusPoolPolicyRule
{
    public List<string> Check(PendingBonusPoolPolicy item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending bonus pool policy.</summary>
public sealed record PendingBonusPoolPolicyValidator(List<IPendingBonusPoolPolicyRule> Rules)
{
    public List<string> Validate(PendingBonusPoolPolicy item)
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

    public void RequireValid(PendingBonusPoolPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBonusPoolPolicy> items)
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

/// <summary>Functions of pending bonus pool policy rules.</summary>
public static class PendingBonusPoolPolicyRules
{
    public static PendingBonusPoolPolicyValidator DefaultPendingBonusPoolPolicyValidator()
    {
        return new PendingBonusPoolPolicyValidator(
            new List<IPendingBonusPoolPolicyRule>
            {
                new PendingBonusPoolPolicyUnitLengthRule(86400),
                new PendingBonusPoolPolicyCodeLengthRule(32),
                new PendingBonusPoolPolicyTitleLengthRule(80),
                new PendingBonusPoolPolicySalaryBandLengthRule(64),
                new PendingBonusPoolPolicyRequiredUnitRule("pending bonus pool policy needs a value"),
            });
    }
}
