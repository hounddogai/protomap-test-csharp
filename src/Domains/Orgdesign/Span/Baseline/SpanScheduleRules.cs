namespace Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>A rule a span schedule must pass. Check returns its problems, or none.</summary>
public interface ISpanScheduleRule
{
    List<string> Check(SpanSchedule item);
}

public sealed record SpanScheduleDepartmentLengthRule(int MaxLength) : ISpanScheduleRule
{
    public List<string> Check(SpanSchedule item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanScheduleRequiredUnitRule(string Message) : ISpanScheduleRule
{
    public List<string> Check(SpanSchedule item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanScheduleCodePrefixRule(string Prefix) : ISpanScheduleRule
{
    public List<string> Check(SpanSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span schedule.</summary>
public sealed record SpanScheduleValidator(List<ISpanScheduleRule> Rules)
{
    public List<string> Validate(SpanSchedule item)
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

    public void RequireValid(SpanSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanSchedule> items)
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

/// <summary>Functions of span schedule rules.</summary>
public static class SpanScheduleRules
{
    public static SpanScheduleValidator DefaultSpanScheduleValidator()
    {
        return new SpanScheduleValidator(
            new List<ISpanScheduleRule>
            {
                new SpanScheduleDepartmentLengthRule(1000),
                new SpanScheduleRequiredUnitRule("span schedule needs a value"),
                new SpanScheduleCodePrefixRule("REF"),
            });
    }
}
