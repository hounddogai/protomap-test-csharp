namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>A rule a span batch must pass. Check returns its problems, or none.</summary>
public interface ISpanBatchRule
{
    List<string> Check(SpanBatch item);
}

public sealed record SpanBatchCodeLengthRule(int MaxLength) : ISpanBatchRule
{
    public List<string> Check(SpanBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBatchNameLengthRule(int MaxLength) : ISpanBatchRule
{
    public List<string> Check(SpanBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBatchLabelLengthRule(int MaxLength) : ISpanBatchRule
{
    public List<string> Check(SpanBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanBatchRequiredNameRule(string Message) : ISpanBatchRule
{
    public List<string> Check(SpanBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span batch.</summary>
public sealed record SpanBatchValidator(List<ISpanBatchRule> Rules)
{
    public List<string> Validate(SpanBatch item)
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

    public void RequireValid(SpanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanBatch> items)
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

/// <summary>Functions of span batch rules.</summary>
public static class SpanBatchRules
{
    public static SpanBatchValidator DefaultSpanBatchValidator()
    {
        return new SpanBatchValidator(
            new List<ISpanBatchRule>
            {
                new SpanBatchCodeLengthRule(1000),
                new SpanBatchNameLengthRule(86400),
                new SpanBatchLabelLengthRule(1000),
                new SpanBatchRequiredNameRule("span batch needs this field"),
            });
    }
}
