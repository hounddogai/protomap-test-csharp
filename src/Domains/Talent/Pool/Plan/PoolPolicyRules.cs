namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool policy must pass. Check returns its problems, or none.</summary>
public interface IPoolPolicyRule
{
    List<string> Check(PoolPolicy item);
}

public sealed record PoolPolicyCapacityRangeRule(int Min, int Max) : IPoolPolicyRule
{
    public List<string> Check(PoolPolicy item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolPolicyAllowedNameRule(List<string> Allowed) : IPoolPolicyRule
{
    public List<string> Check(PoolPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PoolPolicyRequiredDepartmentRule(string Message) : IPoolPolicyRule
{
    public List<string> Check(PoolPolicy item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolPolicyRequiredNameRule(string Message) : IPoolPolicyRule
{
    public List<string> Check(PoolPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolPolicyCodeLengthRule(int MaxLength) : IPoolPolicyRule
{
    public List<string> Check(PoolPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool policy.</summary>
public sealed record PoolPolicyValidator(List<IPoolPolicyRule> Rules)
{
    public List<string> Validate(PoolPolicy item)
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

    public void RequireValid(PoolPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolPolicy> items)
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

/// <summary>Functions of pool policy rules.</summary>
public static class PoolPolicyRules
{
    public static PoolPolicyValidator DefaultPoolPolicyValidator()
    {
        return new PoolPolicyValidator(
            new List<IPoolPolicyRule>
            {
                new PoolPolicyCapacityRangeRule(32, 64),
                new PoolPolicyAllowedNameRule(new List<string>()),
                new PoolPolicyRequiredDepartmentRule("pool policy needs a value"),
                new PoolPolicyRequiredNameRule("pool policy needs an entry"),
                new PoolPolicyCodeLengthRule(80),
            });
    }
}
