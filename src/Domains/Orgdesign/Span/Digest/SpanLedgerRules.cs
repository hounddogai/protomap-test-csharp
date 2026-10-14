namespace Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>A rule a span ledger must pass. Check returns its problems, or none.</summary>
public interface ISpanLedgerRule
{
    List<string> Check(SpanLedger item);
}

public sealed record SpanLedgerSegmentLengthRule(int MaxLength) : ISpanLedgerRule
{
    public List<string> Check(SpanLedger item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanLedgerCodeLengthRule(int MaxLength) : ISpanLedgerRule
{
    public List<string> Check(SpanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanLedgerLeaveDaysRangeRule(int Min, int Max) : ISpanLedgerRule
{
    public List<string> Check(SpanLedger item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SpanLedgerRequiredNameRule(string Message) : ISpanLedgerRule
{
    public List<string> Check(SpanLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanLedgerRequiredSegmentRule(string Message) : ISpanLedgerRule
{
    public List<string> Check(SpanLedger item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanLedgerCreatedAtRangeRule(int Min, int Max) : ISpanLedgerRule
{
    public List<string> Check(SpanLedger item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a span ledger.</summary>
public sealed record SpanLedgerValidator(List<ISpanLedgerRule> Rules)
{
    public List<string> Validate(SpanLedger item)
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

    public void RequireValid(SpanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanLedger> items)
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

/// <summary>Functions of span ledger rules.</summary>
public static class SpanLedgerRules
{
    public static SpanLedgerValidator DefaultSpanLedgerValidator()
    {
        return new SpanLedgerValidator(
            new List<ISpanLedgerRule>
            {
                new SpanLedgerSegmentLengthRule(3600),
                new SpanLedgerCodeLengthRule(32),
                new SpanLedgerLeaveDaysRangeRule(16, 255),
                new SpanLedgerRequiredNameRule("span ledger needs this field"),
                new SpanLedgerRequiredSegmentRule("span ledger needs a value"),
                new SpanLedgerCreatedAtRangeRule(32, 1000),
            });
    }
}
