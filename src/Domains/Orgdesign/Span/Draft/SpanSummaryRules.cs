namespace Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>A rule a span summary must pass. Check returns its problems, or none.</summary>
public interface ISpanSummaryRule
{
    List<string> Check(SpanSummary item);
}

public sealed record SpanSummaryLimitRangeRule(int Min, int Max) : ISpanSummaryRule
{
    public List<string> Check(SpanSummary item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SpanSummaryCostCenterLengthRule(int MaxLength) : ISpanSummaryRule
{
    public List<string> Check(SpanSummary item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanSummaryRequiredLabelRule(string Message) : ISpanSummaryRule
{
    public List<string> Check(SpanSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span summary.</summary>
public sealed record SpanSummaryValidator(List<ISpanSummaryRule> Rules)
{
    public List<string> Validate(SpanSummary item)
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

    public void RequireValid(SpanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanSummary> items)
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

/// <summary>Functions of span summary rules.</summary>
public static class SpanSummaryRules
{
    public static SpanSummaryValidator DefaultSpanSummaryValidator()
    {
        return new SpanSummaryValidator(
            new List<ISpanSummaryRule>
            {
                new SpanSummaryLimitRangeRule(255, 3600),
                new SpanSummaryCostCenterLengthRule(80),
                new SpanSummaryRequiredLabelRule("span summary needs an entry"),
            });
    }
}
