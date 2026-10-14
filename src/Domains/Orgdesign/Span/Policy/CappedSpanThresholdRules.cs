namespace Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>A rule a capped span threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedSpanThresholdRule
{
    List<string> Check(CappedSpanThreshold item);
}

public sealed record CappedSpanThresholdCodePrefixRule(string Prefix) : ICappedSpanThresholdRule
{
    public List<string> Check(CappedSpanThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanThresholdCodeLengthRule(int MaxLength) : ICappedSpanThresholdRule
{
    public List<string> Check(CappedSpanThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanThresholdTitleLengthRule(int MaxLength) : ICappedSpanThresholdRule
{
    public List<string> Check(CappedSpanThreshold item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSpanThresholdRequiredTitleRule(string Message) : ICappedSpanThresholdRule
{
    public List<string> Check(CappedSpanThreshold item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped span threshold.</summary>
public sealed record CappedSpanThresholdValidator(List<ICappedSpanThresholdRule> Rules)
{
    public List<string> Validate(CappedSpanThreshold item)
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

    public void RequireValid(CappedSpanThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSpanThreshold> items)
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

/// <summary>Functions of capped span threshold rules.</summary>
public static class CappedSpanThresholdRules
{
    public static CappedSpanThresholdValidator DefaultCappedSpanThresholdValidator()
    {
        return new CappedSpanThresholdValidator(
            new List<ICappedSpanThresholdRule>
            {
                new CappedSpanThresholdCodePrefixRule("Q"),
                new CappedSpanThresholdCodeLengthRule(3600),
                new CappedSpanThresholdTitleLengthRule(1000),
                new CappedSpanThresholdRequiredTitleRule("capped span threshold needs an entry"),
            });
    }
}
