namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark milestone must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkMilestoneRule
{
    List<string> Check(BenchmarkMilestone item);
}

public sealed record BenchmarkMilestoneCodeLengthRule(int MaxLength) : IBenchmarkMilestoneRule
{
    public List<string> Check(BenchmarkMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMilestoneRequiredManagerIdRule(string Message) : IBenchmarkMilestoneRule
{
    public List<string> Check(BenchmarkMilestone item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMilestoneVisibleGuardRule(double Limit) : IBenchmarkMilestoneRule
{
    public List<string> Check(BenchmarkMilestone item)
    {
        if (item.Visible && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMilestoneRequiredCodeRule(string Message) : IBenchmarkMilestoneRule
{
    public List<string> Check(BenchmarkMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMilestoneScoreRangeRule(double Min, double Max) : IBenchmarkMilestoneRule
{
    public List<string> Check(BenchmarkMilestone item)
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

public sealed record BenchmarkMilestoneAllowedLabelRule(List<string> Allowed) : IBenchmarkMilestoneRule
{
    public List<string> Check(BenchmarkMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark milestone.</summary>
public sealed record BenchmarkMilestoneValidator(List<IBenchmarkMilestoneRule> Rules)
{
    public List<string> Validate(BenchmarkMilestone item)
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

    public void RequireValid(BenchmarkMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkMilestone> items)
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

/// <summary>Functions of benchmark milestone rules.</summary>
public static class BenchmarkMilestoneRules
{
    public static BenchmarkMilestoneValidator DefaultBenchmarkMilestoneValidator()
    {
        return new BenchmarkMilestoneValidator(
            new List<IBenchmarkMilestoneRule>
            {
                new BenchmarkMilestoneCodeLengthRule(1000),
                new BenchmarkMilestoneRequiredManagerIdRule("benchmark milestone needs a value"),
                new BenchmarkMilestoneVisibleGuardRule(7.9),
                new BenchmarkMilestoneRequiredCodeRule("benchmark milestone needs an entry"),
                new BenchmarkMilestoneScoreRangeRule(6.5, 2434.1),
                new BenchmarkMilestoneAllowedLabelRule(new List<string> { "scheduled", "capped", "tiered" }),
            });
    }
}
