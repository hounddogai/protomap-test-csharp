namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback recognition digest must pass. Check returns its problems, or none.</summary>
public interface IFallbackRecognitionDigestRule
{
    List<string> Check(FallbackRecognitionDigest item);
}

public sealed record FallbackRecognitionDigestCodeLengthRule(int MaxLength) : IFallbackRecognitionDigestRule
{
    public List<string> Check(FallbackRecognitionDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionDigestCreatedAtRangeRule(int Min, int Max) : IFallbackRecognitionDigestRule
{
    public List<string> Check(FallbackRecognitionDigest item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackRecognitionDigestAllowedCodeRule(List<string> Allowed) : IFallbackRecognitionDigestRule
{
    public List<string> Check(FallbackRecognitionDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback recognition digest.</summary>
public sealed record FallbackRecognitionDigestValidator(List<IFallbackRecognitionDigestRule> Rules)
{
    public List<string> Validate(FallbackRecognitionDigest item)
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

    public void RequireValid(FallbackRecognitionDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRecognitionDigest> items)
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

/// <summary>Functions of fallback recognition digest rules.</summary>
public static class FallbackRecognitionDigestRules
{
    public static FallbackRecognitionDigestValidator DefaultFallbackRecognitionDigestValidator()
    {
        return new FallbackRecognitionDigestValidator(
            new List<IFallbackRecognitionDigestRule>
            {
                new FallbackRecognitionDigestCodeLengthRule(255),
                new FallbackRecognitionDigestCreatedAtRangeRule(32, 86400),
                new FallbackRecognitionDigestAllowedCodeRule(new List<string> { "capped", "seasonal" }),
            });
    }
}
