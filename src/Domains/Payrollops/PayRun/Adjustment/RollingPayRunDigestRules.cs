namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>A rule a rolling pay run digest must pass. Check returns its problems, or none.</summary>
public interface IRollingPayRunDigestRule
{
    List<string> Check(RollingPayRunDigest item);
}

public sealed record RollingPayRunDigestTenureMonthsRangeRule(int Min, int Max) : IRollingPayRunDigestRule
{
    public List<string> Check(RollingPayRunDigest item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingPayRunDigestPriorityRangeRule(int Min, int Max) : IRollingPayRunDigestRule
{
    public List<string> Check(RollingPayRunDigest item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunDigestRevisionRangeRule(int Min, int Max) : IRollingPayRunDigestRule
{
    public List<string> Check(RollingPayRunDigest item)
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

public sealed record RollingPayRunDigestRequiredLocationRule(string Message) : IRollingPayRunDigestRule
{
    public List<string> Check(RollingPayRunDigest item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPayRunDigestRequiredTitleRule(string Message) : IRollingPayRunDigestRule
{
    public List<string> Check(RollingPayRunDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pay run digest.</summary>
public sealed record RollingPayRunDigestValidator(List<IRollingPayRunDigestRule> Rules)
{
    public List<string> Validate(RollingPayRunDigest item)
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

    public void RequireValid(RollingPayRunDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPayRunDigest> items)
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

/// <summary>Functions of rolling pay run digest rules.</summary>
public static class RollingPayRunDigestRules
{
    public static RollingPayRunDigestValidator DefaultRollingPayRunDigestValidator()
    {
        return new RollingPayRunDigestValidator(
            new List<IRollingPayRunDigestRule>
            {
                new RollingPayRunDigestTenureMonthsRangeRule(32, 1000),
                new RollingPayRunDigestPriorityRangeRule(16, 32),
                new RollingPayRunDigestRevisionRangeRule(32, 120),
                new RollingPayRunDigestRequiredLocationRule("rolling pay run digest needs this field"),
                new RollingPayRunDigestRequiredTitleRule("rolling pay run digest needs a value"),
            });
    }
}
