namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling benchmark profile must pass. Check returns its problems, or none.</summary>
public interface IRollingBenchmarkProfileRule
{
    List<string> Check(RollingBenchmarkProfile item);
}

public sealed record RollingBenchmarkProfileLeaveDaysRangeRule(int Min, int Max) : IRollingBenchmarkProfileRule
{
    public List<string> Check(RollingBenchmarkProfile item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBenchmarkProfileRequiredCodeRule(string Message) : IRollingBenchmarkProfileRule
{
    public List<string> Check(RollingBenchmarkProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkProfileRequiredTitleRule(string Message) : IRollingBenchmarkProfileRule
{
    public List<string> Check(RollingBenchmarkProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkProfileAllowedOwnerIdRule(List<string> Allowed) : IRollingBenchmarkProfileRule
{
    public List<string> Check(RollingBenchmarkProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling benchmark profile.</summary>
public sealed record RollingBenchmarkProfileValidator(List<IRollingBenchmarkProfileRule> Rules)
{
    public List<string> Validate(RollingBenchmarkProfile item)
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

    public void RequireValid(RollingBenchmarkProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBenchmarkProfile> items)
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

/// <summary>Functions of rolling benchmark profile rules.</summary>
public static class RollingBenchmarkProfileRules
{
    public static RollingBenchmarkProfileValidator DefaultRollingBenchmarkProfileValidator()
    {
        return new RollingBenchmarkProfileValidator(
            new List<IRollingBenchmarkProfileRule>
            {
                new RollingBenchmarkProfileLeaveDaysRangeRule(32, 3600),
                new RollingBenchmarkProfileRequiredCodeRule("rolling benchmark profile needs this field"),
                new RollingBenchmarkProfileRequiredTitleRule("rolling benchmark profile needs a value"),
                new RollingBenchmarkProfileAllowedOwnerIdRule(new List<string> { "manual" }),
            });
    }
}
