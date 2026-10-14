namespace Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>A rule a equity grant bundle must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantBundleRule
{
    List<string> Check(EquityGrantBundle item);
}

public sealed record EquityGrantBundleSalaryBandLengthRule(int MaxLength) : IEquityGrantBundleRule
{
    public List<string> Check(EquityGrantBundle item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBundleCodeLengthRule(int MaxLength) : IEquityGrantBundleRule
{
    public List<string> Check(EquityGrantBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBundleRequiredCodeRule(string Message) : IEquityGrantBundleRule
{
    public List<string> Check(EquityGrantBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBundleRetriesRangeRule(int Min, int Max) : IEquityGrantBundleRule
{
    public List<string> Check(EquityGrantBundle item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantBundleCodePrefixRule(string Prefix) : IEquityGrantBundleRule
{
    public List<string> Check(EquityGrantBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantBundleRequiredNotesRule(string Message) : IEquityGrantBundleRule
{
    public List<string> Check(EquityGrantBundle item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant bundle.</summary>
public sealed record EquityGrantBundleValidator(List<IEquityGrantBundleRule> Rules)
{
    public List<string> Validate(EquityGrantBundle item)
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

    public void RequireValid(EquityGrantBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantBundle> items)
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

/// <summary>Functions of equity grant bundle rules.</summary>
public static class EquityGrantBundleRules
{
    public static EquityGrantBundleValidator DefaultEquityGrantBundleValidator()
    {
        return new EquityGrantBundleValidator(
            new List<IEquityGrantBundleRule>
            {
                new EquityGrantBundleSalaryBandLengthRule(120),
                new EquityGrantBundleCodeLengthRule(86400),
                new EquityGrantBundleRequiredCodeRule("equity grant bundle needs an entry"),
                new EquityGrantBundleRetriesRangeRule(120, 1000),
                new EquityGrantBundleCodePrefixRule("Q"),
                new EquityGrantBundleRequiredNotesRule("equity grant bundle needs a value"),
            });
    }
}
