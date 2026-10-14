namespace Acme.Workforce.Talent.Pool.Window;

/// <summary>A rule a pool schedule must pass. Check returns its problems, or none.</summary>
public interface IPoolScheduleRule
{
    List<string> Check(PoolSchedule item);
}

public sealed record PoolScheduleRequiredCodeRule(string Message) : IPoolScheduleRule
{
    public List<string> Check(PoolSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolScheduleDepartmentLengthRule(int MaxLength) : IPoolScheduleRule
{
    public List<string> Check(PoolSchedule item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolScheduleArchivedGuardRule(int Limit) : IPoolScheduleRule
{
    public List<string> Check(PoolSchedule item)
    {
        if (item.Archived && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PoolScheduleTitleLengthRule(int MaxLength) : IPoolScheduleRule
{
    public List<string> Check(PoolSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolScheduleRequiredPhoneRule(string Message) : IPoolScheduleRule
{
    public List<string> Check(PoolSchedule item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool schedule.</summary>
public sealed record PoolScheduleValidator(List<IPoolScheduleRule> Rules)
{
    public List<string> Validate(PoolSchedule item)
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

    public void RequireValid(PoolSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolSchedule> items)
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

/// <summary>Functions of pool schedule rules.</summary>
public static class PoolScheduleRules
{
    public static PoolScheduleValidator DefaultPoolScheduleValidator()
    {
        return new PoolScheduleValidator(
            new List<IPoolScheduleRule>
            {
                new PoolScheduleRequiredCodeRule("pool schedule needs this field"),
                new PoolScheduleDepartmentLengthRule(120),
                new PoolScheduleArchivedGuardRule(255),
                new PoolScheduleTitleLengthRule(32),
                new PoolScheduleRequiredPhoneRule("pool schedule needs an entry"),
            });
    }
}
