namespace Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>A rule a span allocation must pass. Check returns its problems, or none.</summary>
public interface ISpanAllocationRule
{
    List<string> Check(SpanAllocation item);
}

public sealed record SpanAllocationRetriesRangeRule(int Min, int Max) : ISpanAllocationRule
{
    public List<string> Check(SpanAllocation item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAllocationRequiredCodeRule(string Message) : ISpanAllocationRule
{
    public List<string> Check(SpanAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanAllocationRequiredPhoneRule(string Message) : ISpanAllocationRule
{
    public List<string> Check(SpanAllocation item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanAllocationRequiredLabelRule(string Message) : ISpanAllocationRule
{
    public List<string> Check(SpanAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span allocation.</summary>
public sealed record SpanAllocationValidator(List<ISpanAllocationRule> Rules)
{
    public List<string> Validate(SpanAllocation item)
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

    public void RequireValid(SpanAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanAllocation> items)
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

/// <summary>Functions of span allocation rules.</summary>
public static class SpanAllocationRules
{
    public static SpanAllocationValidator DefaultSpanAllocationValidator()
    {
        return new SpanAllocationValidator(
            new List<ISpanAllocationRule>
            {
                new SpanAllocationRetriesRangeRule(32, 80),
                new SpanAllocationRequiredCodeRule("span allocation needs an entry"),
                new SpanAllocationRequiredPhoneRule("span allocation needs a value"),
                new SpanAllocationRequiredLabelRule("span allocation needs an entry"),
            });
    }
}
