namespace Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>A rule a span assignment must pass. Check returns its problems, or none.</summary>
public interface ISpanAssignmentRule
{
    List<string> Check(SpanAssignment item);
}

public sealed record SpanAssignmentScoreRangeRule(double Min, double Max) : ISpanAssignmentRule
{
    public List<string> Check(SpanAssignment item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAssignmentCodeLengthRule(int MaxLength) : ISpanAssignmentRule
{
    public List<string> Check(SpanAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanAssignmentRequiredCodeRule(string Message) : ISpanAssignmentRule
{
    public List<string> Check(SpanAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanAssignmentUpdatedAtRangeRule(int Min, int Max) : ISpanAssignmentRule
{
    public List<string> Check(SpanAssignment item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span assignment.</summary>
public sealed record SpanAssignmentValidator(List<ISpanAssignmentRule> Rules)
{
    public List<string> Validate(SpanAssignment item)
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

    public void RequireValid(SpanAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanAssignment> items)
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

/// <summary>Functions of span assignment rules.</summary>
public static class SpanAssignmentRules
{
    public static SpanAssignmentValidator DefaultSpanAssignmentValidator()
    {
        return new SpanAssignmentValidator(
            new List<ISpanAssignmentRule>
            {
                new SpanAssignmentScoreRangeRule(9.3, 769.2),
                new SpanAssignmentCodeLengthRule(86400),
                new SpanAssignmentRequiredCodeRule("span assignment needs this field"),
                new SpanAssignmentUpdatedAtRangeRule(80, 1000),
            });
    }
}
