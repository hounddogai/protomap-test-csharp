namespace Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>A rule a adjustment request must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentRequestRule
{
    List<string> Check(AdjustmentRequest item);
}

public sealed record AdjustmentRequestRequiredCodeRule(string Message) : IAdjustmentRequestRule
{
    public List<string> Check(AdjustmentRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRequestRequiredLabelRule(string Message) : IAdjustmentRequestRule
{
    public List<string> Check(AdjustmentRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRequestRequiredBadgeRule(string Message) : IAdjustmentRequestRule
{
    public List<string> Check(AdjustmentRequest item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentRequestCodeLengthRule(int MaxLength) : IAdjustmentRequestRule
{
    public List<string> Check(AdjustmentRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment request.</summary>
public sealed record AdjustmentRequestValidator(List<IAdjustmentRequestRule> Rules)
{
    public List<string> Validate(AdjustmentRequest item)
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

    public void RequireValid(AdjustmentRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentRequest> items)
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

/// <summary>Functions of adjustment request rules.</summary>
public static class AdjustmentRequestRules
{
    public static AdjustmentRequestValidator DefaultAdjustmentRequestValidator()
    {
        return new AdjustmentRequestValidator(
            new List<IAdjustmentRequestRule>
            {
                new AdjustmentRequestRequiredCodeRule("adjustment request needs this field"),
                new AdjustmentRequestRequiredLabelRule("adjustment request needs an entry"),
                new AdjustmentRequestRequiredBadgeRule("adjustment request needs a value"),
                new AdjustmentRequestCodeLengthRule(16),
            });
    }
}
