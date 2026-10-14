namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition bundle must pass. Check returns its problems, or none.</summary>
public interface IRecognitionBundleRule
{
    List<string> Check(RecognitionBundle item);
}

public sealed record RecognitionBundleEndsAtRangeRule(int Min, int Max) : IRecognitionBundleRule
{
    public List<string> Check(RecognitionBundle item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RecognitionBundleChecksumLengthRule(int MaxLength) : IRecognitionBundleRule
{
    public List<string> Check(RecognitionBundle item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBundleNameLengthRule(int MaxLength) : IRecognitionBundleRule
{
    public List<string> Check(RecognitionBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBundleStartsAtRangeRule(int Min, int Max) : IRecognitionBundleRule
{
    public List<string> Check(RecognitionBundle item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBundleCodeLengthRule(int MaxLength) : IRecognitionBundleRule
{
    public List<string> Check(RecognitionBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionBundleAllowedChecksumRule(List<string> Allowed) : IRecognitionBundleRule
{
    public List<string> Check(RecognitionBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition bundle.</summary>
public sealed record RecognitionBundleValidator(List<IRecognitionBundleRule> Rules)
{
    public List<string> Validate(RecognitionBundle item)
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

    public void RequireValid(RecognitionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionBundle> items)
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

/// <summary>Functions of recognition bundle rules.</summary>
public static class RecognitionBundleRules
{
    public static RecognitionBundleValidator DefaultRecognitionBundleValidator()
    {
        return new RecognitionBundleValidator(
            new List<IRecognitionBundleRule>
            {
                new RecognitionBundleEndsAtRangeRule(16, 120),
                new RecognitionBundleChecksumLengthRule(32),
                new RecognitionBundleNameLengthRule(3600),
                new RecognitionBundleStartsAtRangeRule(16, 120),
                new RecognitionBundleCodeLengthRule(80),
                new RecognitionBundleAllowedChecksumRule(new List<string> { "adaptive" }),
            });
    }
}
