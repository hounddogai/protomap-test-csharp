namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>A rule a global benchmark policy must pass. Check returns its problems, or none.</summary>
public interface IGlobalBenchmarkPolicyRule
{
    List<string> Check(GlobalBenchmarkPolicy item);
}

public sealed record GlobalBenchmarkPolicyCategoryLengthRule(int MaxLength) : IGlobalBenchmarkPolicyRule
{
    public List<string> Check(GlobalBenchmarkPolicy item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkPolicyCodeLengthRule(int MaxLength) : IGlobalBenchmarkPolicyRule
{
    public List<string> Check(GlobalBenchmarkPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkPolicyThresholdRangeRule(double Min, double Max) : IGlobalBenchmarkPolicyRule
{
    public List<string> Check(GlobalBenchmarkPolicy item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkPolicyNameLengthRule(int MaxLength) : IGlobalBenchmarkPolicyRule
{
    public List<string> Check(GlobalBenchmarkPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkPolicyCostCenterLengthRule(int MaxLength) : IGlobalBenchmarkPolicyRule
{
    public List<string> Check(GlobalBenchmarkPolicy item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBenchmarkPolicyRequiredNameRule(string Message) : IGlobalBenchmarkPolicyRule
{
    public List<string> Check(GlobalBenchmarkPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global benchmark policy.</summary>
public sealed record GlobalBenchmarkPolicyValidator(List<IGlobalBenchmarkPolicyRule> Rules)
{
    public List<string> Validate(GlobalBenchmarkPolicy item)
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

    public void RequireValid(GlobalBenchmarkPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBenchmarkPolicy> items)
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

/// <summary>Functions of global benchmark policy rules.</summary>
public static class GlobalBenchmarkPolicyRules
{
    public static GlobalBenchmarkPolicyValidator DefaultGlobalBenchmarkPolicyValidator()
    {
        return new GlobalBenchmarkPolicyValidator(
            new List<IGlobalBenchmarkPolicyRule>
            {
                new GlobalBenchmarkPolicyCategoryLengthRule(86400),
                new GlobalBenchmarkPolicyCodeLengthRule(255),
                new GlobalBenchmarkPolicyThresholdRangeRule(9.3, 3587.5),
                new GlobalBenchmarkPolicyNameLengthRule(255),
                new GlobalBenchmarkPolicyCostCenterLengthRule(80),
                new GlobalBenchmarkPolicyRequiredNameRule("global benchmark policy needs an entry"),
            });
    }
}
