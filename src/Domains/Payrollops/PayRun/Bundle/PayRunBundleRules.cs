namespace Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>A rule a pay run bundle must pass. Check returns its problems, or none.</summary>
public interface IPayRunBundleRule
{
    List<string> Check(PayRunBundle item);
}

public sealed record PayRunBundleCodePrefixRule(string Prefix) : IPayRunBundleRule
{
    public List<string> Check(PayRunBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBundleCreatedAtRangeRule(int Min, int Max) : IPayRunBundleRule
{
    public List<string> Check(PayRunBundle item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBundleLocationLengthRule(int MaxLength) : IPayRunBundleRule
{
    public List<string> Check(PayRunBundle item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBundleRequiredCodeRule(string Message) : IPayRunBundleRule
{
    public List<string> Check(PayRunBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunBundleTitleLengthRule(int MaxLength) : IPayRunBundleRule
{
    public List<string> Check(PayRunBundle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunBundleCodeLengthRule(int MaxLength) : IPayRunBundleRule
{
    public List<string> Check(PayRunBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run bundle.</summary>
public sealed record PayRunBundleValidator(List<IPayRunBundleRule> Rules)
{
    public List<string> Validate(PayRunBundle item)
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

    public void RequireValid(PayRunBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunBundle> items)
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

/// <summary>Functions of pay run bundle rules.</summary>
public static class PayRunBundleRules
{
    public static PayRunBundleValidator DefaultPayRunBundleValidator()
    {
        return new PayRunBundleValidator(
            new List<IPayRunBundleRule>
            {
                new PayRunBundleCodePrefixRule("OPS"),
                new PayRunBundleCreatedAtRangeRule(1000, 86400),
                new PayRunBundleLocationLengthRule(16),
                new PayRunBundleRequiredCodeRule("pay run bundle needs an entry"),
                new PayRunBundleTitleLengthRule(32),
                new PayRunBundleCodeLengthRule(1000),
            });
    }
}
