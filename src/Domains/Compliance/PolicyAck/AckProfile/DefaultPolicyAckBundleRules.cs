namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>A rule a default policy ack bundle must pass. Check returns its problems, or none.</summary>
public interface IDefaultPolicyAckBundleRule
{
    List<string> Check(DefaultPolicyAckBundle item);
}

public sealed record DefaultPolicyAckBundleBudgetRangeRule(double Min, double Max) : IDefaultPolicyAckBundleRule
{
    public List<string> Check(DefaultPolicyAckBundle item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPolicyAckBundleRequiredTitleRule(string Message) : IDefaultPolicyAckBundleRule
{
    public List<string> Check(DefaultPolicyAckBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckBundleTitleLengthRule(int MaxLength) : IDefaultPolicyAckBundleRule
{
    public List<string> Check(DefaultPolicyAckBundle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckBundleRequiredCodeRule(string Message) : IDefaultPolicyAckBundleRule
{
    public List<string> Check(DefaultPolicyAckBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default policy ack bundle.</summary>
public sealed record DefaultPolicyAckBundleValidator(List<IDefaultPolicyAckBundleRule> Rules)
{
    public List<string> Validate(DefaultPolicyAckBundle item)
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

    public void RequireValid(DefaultPolicyAckBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPolicyAckBundle> items)
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

/// <summary>Functions of default policy ack bundle rules.</summary>
public static class DefaultPolicyAckBundleRules
{
    public static DefaultPolicyAckBundleValidator DefaultDefaultPolicyAckBundleValidator()
    {
        return new DefaultPolicyAckBundleValidator(
            new List<IDefaultPolicyAckBundleRule>
            {
                new DefaultPolicyAckBundleBudgetRangeRule(1.8, 4825.6),
                new DefaultPolicyAckBundleRequiredTitleRule("default policy ack bundle needs a value"),
                new DefaultPolicyAckBundleTitleLengthRule(3600),
                new DefaultPolicyAckBundleRequiredCodeRule("default policy ack bundle needs an entry"),
            });
    }
}
