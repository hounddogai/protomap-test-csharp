namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

/// <summary>A rule a manual succession plan batch must pass. Check returns its problems, or none.</summary>
public interface IManualSuccessionPlanBatchRule
{
    List<string> Check(ManualSuccessionPlanBatch item);
}

public sealed record ManualSuccessionPlanBatchLabelLengthRule(int MaxLength) : IManualSuccessionPlanBatchRule
{
    public List<string> Check(ManualSuccessionPlanBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSuccessionPlanBatchRequiredLabelRule(string Message) : IManualSuccessionPlanBatchRule
{
    public List<string> Check(ManualSuccessionPlanBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSuccessionPlanBatchCodePrefixRule(string Prefix) : IManualSuccessionPlanBatchRule
{
    public List<string> Check(ManualSuccessionPlanBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSuccessionPlanBatchCodeLengthRule(int MaxLength) : IManualSuccessionPlanBatchRule
{
    public List<string> Check(ManualSuccessionPlanBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual succession plan batch.</summary>
public sealed record ManualSuccessionPlanBatchValidator(List<IManualSuccessionPlanBatchRule> Rules)
{
    public List<string> Validate(ManualSuccessionPlanBatch item)
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

    public void RequireValid(ManualSuccessionPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualSuccessionPlanBatch> items)
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

/// <summary>Functions of manual succession plan batch rules.</summary>
public static class ManualSuccessionPlanBatchRules
{
    public static ManualSuccessionPlanBatchValidator DefaultManualSuccessionPlanBatchValidator()
    {
        return new ManualSuccessionPlanBatchValidator(
            new List<IManualSuccessionPlanBatchRule>
            {
                new ManualSuccessionPlanBatchLabelLengthRule(64),
                new ManualSuccessionPlanBatchRequiredLabelRule("manual succession plan batch needs a value"),
                new ManualSuccessionPlanBatchCodePrefixRule("X"),
                new ManualSuccessionPlanBatchCodeLengthRule(86400),
            });
    }
}
