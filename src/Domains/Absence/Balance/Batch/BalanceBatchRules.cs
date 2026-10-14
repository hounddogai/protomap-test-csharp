namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance batch must pass. Check returns its problems, or none.</summary>
public interface IBalanceBatchRule
{
    List<string> Check(BalanceBatch item);
}

public sealed record BalanceBatchWindowDaysRangeRule(int Min, int Max) : IBalanceBatchRule
{
    public List<string> Check(BalanceBatch item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalanceBatchRevisionRangeRule(int Min, int Max) : IBalanceBatchRule
{
    public List<string> Check(BalanceBatch item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBatchAllowedUnitRule(List<string> Allowed) : IBalanceBatchRule
{
    public List<string> Check(BalanceBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBatchDescriptionLengthRule(int MaxLength) : IBalanceBatchRule
{
    public List<string> Check(BalanceBatch item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance batch.</summary>
public sealed record BalanceBatchValidator(List<IBalanceBatchRule> Rules)
{
    public List<string> Validate(BalanceBatch item)
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

    public void RequireValid(BalanceBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceBatch> items)
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

/// <summary>Functions of balance batch rules.</summary>
public static class BalanceBatchRules
{
    public static BalanceBatchValidator DefaultBalanceBatchValidator()
    {
        return new BalanceBatchValidator(
            new List<IBalanceBatchRule>
            {
                new BalanceBatchWindowDaysRangeRule(255, 1000),
                new BalanceBatchRevisionRangeRule(1000, 1000),
                new BalanceBatchAllowedUnitRule(new List<string> { "seasonal", "capped" }),
                new BalanceBatchDescriptionLengthRule(120),
            });
    }
}
