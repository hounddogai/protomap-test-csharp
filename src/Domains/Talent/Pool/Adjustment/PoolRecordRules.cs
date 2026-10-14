namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool record must pass. Check returns its problems, or none.</summary>
public interface IPoolRecordRule
{
    List<string> Check(PoolRecord item);
}

public sealed record PoolRecordAllowedReviewerRule(List<string> Allowed) : IPoolRecordRule
{
    public List<string> Check(PoolRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PoolRecordRequiredCodeRule(string Message) : IPoolRecordRule
{
    public List<string> Check(PoolRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolRecordReviewerLengthRule(int MaxLength) : IPoolRecordRule
{
    public List<string> Check(PoolRecord item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool record.</summary>
public sealed record PoolRecordValidator(List<IPoolRecordRule> Rules)
{
    public List<string> Validate(PoolRecord item)
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

    public void RequireValid(PoolRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolRecord> items)
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

/// <summary>Functions of pool record rules.</summary>
public static class PoolRecordRules
{
    public static PoolRecordValidator DefaultPoolRecordValidator()
    {
        return new PoolRecordValidator(
            new List<IPoolRecordRule>
            {
                new PoolRecordAllowedReviewerRule(new List<string> { "adaptive", "legacy", "seasonal" }),
                new PoolRecordRequiredCodeRule("pool record needs an entry"),
                new PoolRecordReviewerLengthRule(86400),
            });
    }
}
