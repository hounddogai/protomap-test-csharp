namespace Acme.Workforce.Absence.Policy.Batch;

/// <summary>A rule a policy batch must pass. Check returns its problems, or none.</summary>
public interface IPolicyBatchRule
{
    List<string> Check(PolicyBatch item);
}

public sealed record PolicyBatchCeilingRangeRule(double Min, double Max) : IPolicyBatchRule
{
    public List<string> Check(PolicyBatch item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyBatchRequiredExternalRefRule(string Message) : IPolicyBatchRule
{
    public List<string> Check(PolicyBatch item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyBatchLabelLengthRule(int MaxLength) : IPolicyBatchRule
{
    public List<string> Check(PolicyBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyBatchLeaveDaysRangeRule(int Min, int Max) : IPolicyBatchRule
{
    public List<string> Check(PolicyBatch item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy batch.</summary>
public sealed record PolicyBatchValidator(List<IPolicyBatchRule> Rules)
{
    public List<string> Validate(PolicyBatch item)
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

    public void RequireValid(PolicyBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyBatch> items)
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

/// <summary>Functions of policy batch rules.</summary>
public static class PolicyBatchRules
{
    public static PolicyBatchValidator DefaultPolicyBatchValidator()
    {
        return new PolicyBatchValidator(
            new List<IPolicyBatchRule>
            {
                new PolicyBatchCeilingRangeRule(5.3, 4438.0),
                new PolicyBatchRequiredExternalRefRule("policy batch needs a value"),
                new PolicyBatchLabelLengthRule(255),
                new PolicyBatchLeaveDaysRangeRule(3600, 86400),
            });
    }
}
