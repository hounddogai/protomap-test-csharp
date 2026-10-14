namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition digest must pass. Check returns its problems, or none.</summary>
public interface IRecognitionDigestRule
{
    List<string> Check(RecognitionDigest item);
}

public sealed record RecognitionDigestCodeLengthRule(int MaxLength) : IRecognitionDigestRule
{
    public List<string> Check(RecognitionDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDigestAllowedSegmentRule(List<string> Allowed) : IRecognitionDigestRule
{
    public List<string> Check(RecognitionDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDigestSegmentLengthRule(int MaxLength) : IRecognitionDigestRule
{
    public List<string> Check(RecognitionDigest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionDigestTitleLengthRule(int MaxLength) : IRecognitionDigestRule
{
    public List<string> Check(RecognitionDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition digest.</summary>
public sealed record RecognitionDigestValidator(List<IRecognitionDigestRule> Rules)
{
    public List<string> Validate(RecognitionDigest item)
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

    public void RequireValid(RecognitionDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionDigest> items)
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

/// <summary>Functions of recognition digest rules.</summary>
public static class RecognitionDigestRules
{
    public static RecognitionDigestValidator DefaultRecognitionDigestValidator()
    {
        return new RecognitionDigestValidator(
            new List<IRecognitionDigestRule>
            {
                new RecognitionDigestCodeLengthRule(3600),
                new RecognitionDigestAllowedSegmentRule(new List<string>()),
                new RecognitionDigestSegmentLengthRule(86400),
                new RecognitionDigestTitleLengthRule(86400),
            });
    }
}
