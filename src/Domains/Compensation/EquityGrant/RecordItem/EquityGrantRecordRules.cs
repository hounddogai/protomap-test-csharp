namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>A rule a equity grant record must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantRecordRule
{
    List<string> Check(EquityGrantRecord item);
}

public sealed record EquityGrantRecordWindowRule(int MaxSpan) : IEquityGrantRecordRule
{
    public List<string> Check(EquityGrantRecord item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRecordContactEmailLengthRule(int MaxLength) : IEquityGrantRecordRule
{
    public List<string> Check(EquityGrantRecord item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRecordScoreRangeRule(double Min, double Max) : IEquityGrantRecordRule
{
    public List<string> Check(EquityGrantRecord item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantRecordStartsAtRangeRule(int Min, int Max) : IEquityGrantRecordRule
{
    public List<string> Check(EquityGrantRecord item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRecordCodePrefixRule(string Prefix) : IEquityGrantRecordRule
{
    public List<string> Check(EquityGrantRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRecordEndsAtRangeRule(int Min, int Max) : IEquityGrantRecordRule
{
    public List<string> Check(EquityGrantRecord item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a equity grant record.</summary>
public sealed record EquityGrantRecordValidator(List<IEquityGrantRecordRule> Rules)
{
    public List<string> Validate(EquityGrantRecord item)
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

    public void RequireValid(EquityGrantRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantRecord> items)
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

/// <summary>Functions of equity grant record rules.</summary>
public static class EquityGrantRecordRules
{
    public static EquityGrantRecordValidator DefaultEquityGrantRecordValidator()
    {
        return new EquityGrantRecordValidator(
            new List<IEquityGrantRecordRule>
            {
                new EquityGrantRecordWindowRule(80),
                new EquityGrantRecordContactEmailLengthRule(32),
                new EquityGrantRecordScoreRangeRule(8.3, 4235.4),
                new EquityGrantRecordStartsAtRangeRule(80, 120),
                new EquityGrantRecordCodePrefixRule("X"),
                new EquityGrantRecordEndsAtRangeRule(80, 86400),
            });
    }
}
