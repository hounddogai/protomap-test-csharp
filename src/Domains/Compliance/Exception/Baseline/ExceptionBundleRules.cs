namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a exception bundle must pass. Check returns its problems, or none.</summary>
public interface IExceptionBundleRule
{
    List<string> Check(ExceptionBundle item);
}

public sealed record ExceptionBundleRequiredCodeRule(string Message) : IExceptionBundleRule
{
    public List<string> Check(ExceptionBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBundleCodePrefixRule(string Prefix) : IExceptionBundleRule
{
    public List<string> Check(ExceptionBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBundleRequiredTitleRule(string Message) : IExceptionBundleRule
{
    public List<string> Check(ExceptionBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBundleRateRangeRule(double Min, double Max) : IExceptionBundleRule
{
    public List<string> Check(ExceptionBundle item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionBundleAttemptsRangeRule(int Min, int Max) : IExceptionBundleRule
{
    public List<string> Check(ExceptionBundle item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionBundleAllowedTitleRule(List<string> Allowed) : IExceptionBundleRule
{
    public List<string> Check(ExceptionBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception bundle.</summary>
public sealed record ExceptionBundleValidator(List<IExceptionBundleRule> Rules)
{
    public List<string> Validate(ExceptionBundle item)
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

    public void RequireValid(ExceptionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionBundle> items)
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

/// <summary>Functions of exception bundle rules.</summary>
public static class ExceptionBundleRules
{
    public static ExceptionBundleValidator DefaultExceptionBundleValidator()
    {
        return new ExceptionBundleValidator(
            new List<IExceptionBundleRule>
            {
                new ExceptionBundleRequiredCodeRule("exception bundle needs a value"),
                new ExceptionBundleCodePrefixRule("OPS"),
                new ExceptionBundleRequiredTitleRule("exception bundle needs this field"),
                new ExceptionBundleRateRangeRule(9.3, 3497.9),
                new ExceptionBundleAttemptsRangeRule(32, 80),
                new ExceptionBundleAllowedTitleRule(new List<string>()),
            });
    }
}
