namespace Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>A rule a span profile must pass. Check returns its problems, or none.</summary>
public interface ISpanProfileRule
{
    List<string> Check(SpanProfile item);
}

public sealed record SpanProfileRequiredSourceRule(string Message) : ISpanProfileRule
{
    public List<string> Check(SpanProfile item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanProfileRequiredCodeRule(string Message) : ISpanProfileRule
{
    public List<string> Check(SpanProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanProfileCodeLengthRule(int MaxLength) : ISpanProfileRule
{
    public List<string> Check(SpanProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanProfileRequiredCostCenterRule(string Message) : ISpanProfileRule
{
    public List<string> Check(SpanProfile item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span profile.</summary>
public sealed record SpanProfileValidator(List<ISpanProfileRule> Rules)
{
    public List<string> Validate(SpanProfile item)
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

    public void RequireValid(SpanProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanProfile> items)
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

/// <summary>Functions of span profile rules.</summary>
public static class SpanProfileRules
{
    public static SpanProfileValidator DefaultSpanProfileValidator()
    {
        return new SpanProfileValidator(
            new List<ISpanProfileRule>
            {
                new SpanProfileRequiredSourceRule("span profile needs this field"),
                new SpanProfileRequiredCodeRule("span profile needs this field"),
                new SpanProfileCodeLengthRule(255),
                new SpanProfileRequiredCostCenterRule("span profile needs an entry"),
            });
    }
}
