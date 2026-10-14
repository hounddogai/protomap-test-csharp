namespace Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>A rule a position record must pass. Check returns its problems, or none.</summary>
public interface IPositionRecordRule
{
    List<string> Check(PositionRecord item);
}

public sealed record PositionRecordCodePrefixRule(string Prefix) : IPositionRecordRule
{
    public List<string> Check(PositionRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionRecordPinnedGuardRule(int Limit) : IPositionRecordRule
{
    public List<string> Check(PositionRecord item)
    {
        if (item.Pinned && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PositionRecordEndsAtRangeRule(int Min, int Max) : IPositionRecordRule
{
    public List<string> Check(PositionRecord item)
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

public sealed record PositionRecordRevisionRangeRule(int Min, int Max) : IPositionRecordRule
{
    public List<string> Check(PositionRecord item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionRecordRequiredCodeRule(string Message) : IPositionRecordRule
{
    public List<string> Check(PositionRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionRecordBudgetRangeRule(double Min, double Max) : IPositionRecordRule
{
    public List<string> Check(PositionRecord item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a position record.</summary>
public sealed record PositionRecordValidator(List<IPositionRecordRule> Rules)
{
    public List<string> Validate(PositionRecord item)
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

    public void RequireValid(PositionRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionRecord> items)
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

/// <summary>Functions of position record rules.</summary>
public static class PositionRecordRules
{
    public static PositionRecordValidator DefaultPositionRecordValidator()
    {
        return new PositionRecordValidator(
            new List<IPositionRecordRule>
            {
                new PositionRecordCodePrefixRule("OPS"),
                new PositionRecordPinnedGuardRule(80),
                new PositionRecordEndsAtRangeRule(32, 255),
                new PositionRecordRevisionRangeRule(16, 86400),
                new PositionRecordRequiredCodeRule("position record needs an entry"),
                new PositionRecordBudgetRangeRule(10.0, 897.9),
            });
    }
}
