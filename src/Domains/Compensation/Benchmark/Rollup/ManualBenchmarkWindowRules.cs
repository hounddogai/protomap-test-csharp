namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual benchmark window must pass. Check returns its problems, or none.</summary>
public interface IManualBenchmarkWindowRule
{
    List<string> Check(ManualBenchmarkWindow item);
}

public sealed record ManualBenchmarkWindowRequiredCodeRule(string Message) : IManualBenchmarkWindowRule
{
    public List<string> Check(ManualBenchmarkWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkWindowCodePrefixRule(string Prefix) : IManualBenchmarkWindowRule
{
    public List<string> Check(ManualBenchmarkWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkWindowRequiredTitleRule(string Message) : IManualBenchmarkWindowRule
{
    public List<string> Check(ManualBenchmarkWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkWindowAllowedCodeRule(List<string> Allowed) : IManualBenchmarkWindowRule
{
    public List<string> Check(ManualBenchmarkWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkWindowReviewerLengthRule(int MaxLength) : IManualBenchmarkWindowRule
{
    public List<string> Check(ManualBenchmarkWindow item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkWindowManagerIdLengthRule(int MaxLength) : IManualBenchmarkWindowRule
{
    public List<string> Check(ManualBenchmarkWindow item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual benchmark window.</summary>
public sealed record ManualBenchmarkWindowValidator(List<IManualBenchmarkWindowRule> Rules)
{
    public List<string> Validate(ManualBenchmarkWindow item)
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

    public void RequireValid(ManualBenchmarkWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBenchmarkWindow> items)
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

/// <summary>Functions of manual benchmark window rules.</summary>
public static class ManualBenchmarkWindowRules
{
    public static ManualBenchmarkWindowValidator DefaultManualBenchmarkWindowValidator()
    {
        return new ManualBenchmarkWindowValidator(
            new List<IManualBenchmarkWindowRule>
            {
                new ManualBenchmarkWindowRequiredCodeRule("manual benchmark window needs an entry"),
                new ManualBenchmarkWindowCodePrefixRule("REF"),
                new ManualBenchmarkWindowRequiredTitleRule("manual benchmark window needs an entry"),
                new ManualBenchmarkWindowAllowedCodeRule(new List<string> { "scheduled" }),
                new ManualBenchmarkWindowReviewerLengthRule(32),
                new ManualBenchmarkWindowManagerIdLengthRule(120),
            });
    }
}
