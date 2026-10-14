namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark segment must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkSegmentRule
{
    List<string> Check(BenchmarkSegment item);
}

public sealed record BenchmarkSegmentChecksumLengthRule(int MaxLength) : IBenchmarkSegmentRule
{
    public List<string> Check(BenchmarkSegment item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSegmentRequiredCodeRule(string Message) : IBenchmarkSegmentRule
{
    public List<string> Check(BenchmarkSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSegmentCodeLengthRule(int MaxLength) : IBenchmarkSegmentRule
{
    public List<string> Check(BenchmarkSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSegmentWindowDaysRangeRule(int Min, int Max) : IBenchmarkSegmentRule
{
    public List<string> Check(BenchmarkSegment item)
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

public sealed record BenchmarkSegmentAllowedCodeRule(List<string> Allowed) : IBenchmarkSegmentRule
{
    public List<string> Check(BenchmarkSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark segment.</summary>
public sealed record BenchmarkSegmentValidator(List<IBenchmarkSegmentRule> Rules)
{
    public List<string> Validate(BenchmarkSegment item)
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

    public void RequireValid(BenchmarkSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkSegment> items)
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

/// <summary>Functions of benchmark segment rules.</summary>
public static class BenchmarkSegmentRules
{
    public static BenchmarkSegmentValidator DefaultBenchmarkSegmentValidator()
    {
        return new BenchmarkSegmentValidator(
            new List<IBenchmarkSegmentRule>
            {
                new BenchmarkSegmentChecksumLengthRule(86400),
                new BenchmarkSegmentRequiredCodeRule("benchmark segment needs a value"),
                new BenchmarkSegmentCodeLengthRule(1000),
                new BenchmarkSegmentWindowDaysRangeRule(32, 80),
                new BenchmarkSegmentAllowedCodeRule(new List<string> { "weighted" }),
            });
    }
}
