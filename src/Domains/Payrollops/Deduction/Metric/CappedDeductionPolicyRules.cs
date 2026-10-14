namespace Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>A rule a capped deduction policy must pass. Check returns its problems, or none.</summary>
public interface ICappedDeductionPolicyRule
{
    List<string> Check(CappedDeductionPolicy item);
}

public sealed record CappedDeductionPolicyDescriptionLengthRule(int MaxLength) : ICappedDeductionPolicyRule
{
    public List<string> Check(CappedDeductionPolicy item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionPolicyWindowDaysRangeRule(int Min, int Max) : ICappedDeductionPolicyRule
{
    public List<string> Check(CappedDeductionPolicy item)
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

public sealed record CappedDeductionPolicyCodePrefixRule(string Prefix) : ICappedDeductionPolicyRule
{
    public List<string> Check(CappedDeductionPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionPolicyVersionRangeRule(int Min, int Max) : ICappedDeductionPolicyRule
{
    public List<string> Check(CappedDeductionPolicy item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped deduction policy.</summary>
public sealed record CappedDeductionPolicyValidator(List<ICappedDeductionPolicyRule> Rules)
{
    public List<string> Validate(CappedDeductionPolicy item)
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

    public void RequireValid(CappedDeductionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedDeductionPolicy> items)
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

/// <summary>Functions of capped deduction policy rules.</summary>
public static class CappedDeductionPolicyRules
{
    public static CappedDeductionPolicyValidator DefaultCappedDeductionPolicyValidator()
    {
        return new CappedDeductionPolicyValidator(
            new List<ICappedDeductionPolicyRule>
            {
                new CappedDeductionPolicyDescriptionLengthRule(120),
                new CappedDeductionPolicyWindowDaysRangeRule(80, 120),
                new CappedDeductionPolicyCodePrefixRule("Q"),
                new CappedDeductionPolicyVersionRangeRule(32, 3600),
            });
    }
}
