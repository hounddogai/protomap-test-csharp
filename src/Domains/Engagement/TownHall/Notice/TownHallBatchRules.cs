namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall batch must pass. Check returns its problems, or none.</summary>
public interface ITownHallBatchRule
{
    List<string> Check(TownHallBatch item);
}

public sealed record TownHallBatchCodeLengthRule(int MaxLength) : ITownHallBatchRule
{
    public List<string> Check(TownHallBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBatchCodePrefixRule(string Prefix) : ITownHallBatchRule
{
    public List<string> Check(TownHallBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBatchRequiredEmployeeIdRule(string Message) : ITownHallBatchRule
{
    public List<string> Check(TownHallBatch item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallBatchUpdatedAtRangeRule(int Min, int Max) : ITownHallBatchRule
{
    public List<string> Check(TownHallBatch item)
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

public sealed record TownHallBatchAllowedTitleRule(List<string> Allowed) : ITownHallBatchRule
{
    public List<string> Check(TownHallBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBatchRequiredTitleRule(string Message) : ITownHallBatchRule
{
    public List<string> Check(TownHallBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall batch.</summary>
public sealed record TownHallBatchValidator(List<ITownHallBatchRule> Rules)
{
    public List<string> Validate(TownHallBatch item)
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

    public void RequireValid(TownHallBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallBatch> items)
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

/// <summary>Functions of town hall batch rules.</summary>
public static class TownHallBatchRules
{
    public static TownHallBatchValidator DefaultTownHallBatchValidator()
    {
        return new TownHallBatchValidator(
            new List<ITownHallBatchRule>
            {
                new TownHallBatchCodeLengthRule(80),
                new TownHallBatchCodePrefixRule("OPS"),
                new TownHallBatchRequiredEmployeeIdRule("town hall batch needs an entry"),
                new TownHallBatchUpdatedAtRangeRule(16, 1000),
                new TownHallBatchAllowedTitleRule(new List<string> { "lenient", "legacy" }),
                new TownHallBatchRequiredTitleRule("town hall batch needs a value"),
            });
    }
}
