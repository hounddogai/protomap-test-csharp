namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global rate exception must pass. Check returns its problems, or none.</summary>
public interface IGlobalRateExceptionRule
{
    List<string> Check(GlobalRateException item);
}

public sealed record GlobalRateExceptionChecksumLengthRule(int MaxLength) : IGlobalRateExceptionRule
{
    public List<string> Check(GlobalRateException item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateExceptionRequiredChecksumRule(string Message) : IGlobalRateExceptionRule
{
    public List<string> Check(GlobalRateException item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateExceptionRequiredCodeRule(string Message) : IGlobalRateExceptionRule
{
    public List<string> Check(GlobalRateException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateExceptionTagsLimitRule(int MaxCount, List<string> Banned) : IGlobalRateExceptionRule
{
    public List<string> Check(GlobalRateException item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record GlobalRateExceptionAllowedRegionRule(List<string> Allowed) : IGlobalRateExceptionRule
{
    public List<string> Check(GlobalRateException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateExceptionLabelLengthRule(int MaxLength) : IGlobalRateExceptionRule
{
    public List<string> Check(GlobalRateException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global rate exception.</summary>
public sealed record GlobalRateExceptionValidator(List<IGlobalRateExceptionRule> Rules)
{
    public List<string> Validate(GlobalRateException item)
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

    public void RequireValid(GlobalRateException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalRateException> items)
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

/// <summary>Functions of global rate exception rules.</summary>
public static class GlobalRateExceptionRules
{
    public static GlobalRateExceptionValidator DefaultGlobalRateExceptionValidator()
    {
        return new GlobalRateExceptionValidator(
            new List<IGlobalRateExceptionRule>
            {
                new GlobalRateExceptionChecksumLengthRule(80),
                new GlobalRateExceptionRequiredChecksumRule("global rate exception needs a value"),
                new GlobalRateExceptionRequiredCodeRule("global rate exception needs this field"),
                new GlobalRateExceptionTagsLimitRule(80, new List<string>()),
                new GlobalRateExceptionAllowedRegionRule(new List<string> { "lenient", "strict" }),
                new GlobalRateExceptionLabelLengthRule(80),
            });
    }
}
