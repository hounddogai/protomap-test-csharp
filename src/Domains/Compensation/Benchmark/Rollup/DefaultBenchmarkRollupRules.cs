namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default benchmark rollup must pass. Check returns its problems, or none.</summary>
public interface IDefaultBenchmarkRollupRule
{
    List<string> Check(DefaultBenchmarkRollup item);
}

public sealed record DefaultBenchmarkRollupRequiredCodeRule(string Message) : IDefaultBenchmarkRollupRule
{
    public List<string> Check(DefaultBenchmarkRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBenchmarkRollupAllowedGradeRule(List<string> Allowed) : IDefaultBenchmarkRollupRule
{
    public List<string> Check(DefaultBenchmarkRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBenchmarkRollupSegmentLengthRule(int MaxLength) : IDefaultBenchmarkRollupRule
{
    public List<string> Check(DefaultBenchmarkRollup item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBenchmarkRollupCapacityRangeRule(int Min, int Max) : IDefaultBenchmarkRollupRule
{
    public List<string> Check(DefaultBenchmarkRollup item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultBenchmarkRollupRequiredGradeRule(string Message) : IDefaultBenchmarkRollupRule
{
    public List<string> Check(DefaultBenchmarkRollup item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default benchmark rollup.</summary>
public sealed record DefaultBenchmarkRollupValidator(List<IDefaultBenchmarkRollupRule> Rules)
{
    public List<string> Validate(DefaultBenchmarkRollup item)
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

    public void RequireValid(DefaultBenchmarkRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBenchmarkRollup> items)
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

/// <summary>Functions of default benchmark rollup rules.</summary>
public static class DefaultBenchmarkRollupRules
{
    public static DefaultBenchmarkRollupValidator DefaultDefaultBenchmarkRollupValidator()
    {
        return new DefaultBenchmarkRollupValidator(
            new List<IDefaultBenchmarkRollupRule>
            {
                new DefaultBenchmarkRollupRequiredCodeRule("default benchmark rollup needs an entry"),
                new DefaultBenchmarkRollupAllowedGradeRule(new List<string>()),
                new DefaultBenchmarkRollupSegmentLengthRule(64),
                new DefaultBenchmarkRollupCapacityRangeRule(80, 86400),
                new DefaultBenchmarkRollupRequiredGradeRule("default benchmark rollup needs a value"),
            });
    }
}
