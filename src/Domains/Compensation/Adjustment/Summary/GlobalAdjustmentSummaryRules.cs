namespace Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>A rule a global adjustment summary must pass. Check returns its problems, or none.</summary>
public interface IGlobalAdjustmentSummaryRule
{
    List<string> Check(GlobalAdjustmentSummary item);
}

public sealed record GlobalAdjustmentSummaryTitleLengthRule(int MaxLength) : IGlobalAdjustmentSummaryRule
{
    public List<string> Check(GlobalAdjustmentSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentSummaryRequiredTitleRule(string Message) : IGlobalAdjustmentSummaryRule
{
    public List<string> Check(GlobalAdjustmentSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentSummarySummaryLengthRule(int MaxLength) : IGlobalAdjustmentSummaryRule
{
    public List<string> Check(GlobalAdjustmentSummary item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentSummaryRequiredLocationRule(string Message) : IGlobalAdjustmentSummaryRule
{
    public List<string> Check(GlobalAdjustmentSummary item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global adjustment summary.</summary>
public sealed record GlobalAdjustmentSummaryValidator(List<IGlobalAdjustmentSummaryRule> Rules)
{
    public List<string> Validate(GlobalAdjustmentSummary item)
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

    public void RequireValid(GlobalAdjustmentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAdjustmentSummary> items)
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

/// <summary>Functions of global adjustment summary rules.</summary>
public static class GlobalAdjustmentSummaryRules
{
    public static GlobalAdjustmentSummaryValidator DefaultGlobalAdjustmentSummaryValidator()
    {
        return new GlobalAdjustmentSummaryValidator(
            new List<IGlobalAdjustmentSummaryRule>
            {
                new GlobalAdjustmentSummaryTitleLengthRule(1000),
                new GlobalAdjustmentSummaryRequiredTitleRule("global adjustment summary needs a value"),
                new GlobalAdjustmentSummarySummaryLengthRule(32),
                new GlobalAdjustmentSummaryRequiredLocationRule("global adjustment summary needs an entry"),
            });
    }
}
