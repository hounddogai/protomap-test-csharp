namespace Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>A rule a rolling balance record must pass. Check returns its problems, or none.</summary>
public interface IRollingBalanceRecordRule
{
    List<string> Check(RollingBalanceRecord item);
}

public sealed record RollingBalanceRecordSourceLengthRule(int MaxLength) : IRollingBalanceRecordRule
{
    public List<string> Check(RollingBalanceRecord item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceRecordCodePrefixRule(string Prefix) : IRollingBalanceRecordRule
{
    public List<string> Check(RollingBalanceRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceRecordRequiredSummaryRule(string Message) : IRollingBalanceRecordRule
{
    public List<string> Check(RollingBalanceRecord item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceRecordUpdatedAtRangeRule(int Min, int Max) : IRollingBalanceRecordRule
{
    public List<string> Check(RollingBalanceRecord item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBalanceRecordSequenceRangeRule(int Min, int Max) : IRollingBalanceRecordRule
{
    public List<string> Check(RollingBalanceRecord item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBalanceRecordLabelLengthRule(int MaxLength) : IRollingBalanceRecordRule
{
    public List<string> Check(RollingBalanceRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling balance record.</summary>
public sealed record RollingBalanceRecordValidator(List<IRollingBalanceRecordRule> Rules)
{
    public List<string> Validate(RollingBalanceRecord item)
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

    public void RequireValid(RollingBalanceRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBalanceRecord> items)
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

/// <summary>Functions of rolling balance record rules.</summary>
public static class RollingBalanceRecordRules
{
    public static RollingBalanceRecordValidator DefaultRollingBalanceRecordValidator()
    {
        return new RollingBalanceRecordValidator(
            new List<IRollingBalanceRecordRule>
            {
                new RollingBalanceRecordSourceLengthRule(80),
                new RollingBalanceRecordCodePrefixRule("REF"),
                new RollingBalanceRecordRequiredSummaryRule("rolling balance record needs a value"),
                new RollingBalanceRecordUpdatedAtRangeRule(64, 255),
                new RollingBalanceRecordSequenceRangeRule(80, 86400),
                new RollingBalanceRecordLabelLengthRule(32),
            });
    }
}
