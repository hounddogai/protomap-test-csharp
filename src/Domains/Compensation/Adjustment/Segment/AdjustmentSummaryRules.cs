namespace Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>A rule a adjustment summary must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentSummaryRule
{
    List<string> Check(AdjustmentSummary item);
}

public sealed record AdjustmentSummaryRequiredLabelRule(string Message) : IAdjustmentSummaryRule
{
    public List<string> Check(AdjustmentSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSummaryCodeLengthRule(int MaxLength) : IAdjustmentSummaryRule
{
    public List<string> Check(AdjustmentSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSummaryRequiredCodeRule(string Message) : IAdjustmentSummaryRule
{
    public List<string> Check(AdjustmentSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentSummaryExpiresAtRangeRule(int Min, int Max) : IAdjustmentSummaryRule
{
    public List<string> Check(AdjustmentSummary item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adjustment summary.</summary>
public sealed record AdjustmentSummaryValidator(List<IAdjustmentSummaryRule> Rules)
{
    public List<string> Validate(AdjustmentSummary item)
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

    public void RequireValid(AdjustmentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentSummary> items)
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

/// <summary>Functions of adjustment summary rules.</summary>
public static class AdjustmentSummaryRules
{
    public static AdjustmentSummaryValidator DefaultAdjustmentSummaryValidator()
    {
        return new AdjustmentSummaryValidator(
            new List<IAdjustmentSummaryRule>
            {
                new AdjustmentSummaryRequiredLabelRule("adjustment summary needs this field"),
                new AdjustmentSummaryCodeLengthRule(80),
                new AdjustmentSummaryRequiredCodeRule("adjustment summary needs an entry"),
                new AdjustmentSummaryExpiresAtRangeRule(16, 255),
            });
    }
}
