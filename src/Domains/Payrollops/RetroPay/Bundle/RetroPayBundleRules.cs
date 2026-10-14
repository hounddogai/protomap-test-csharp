namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>A rule a retro pay bundle must pass. Check returns its problems, or none.</summary>
public interface IRetroPayBundleRule
{
    List<string> Check(RetroPayBundle item);
}

public sealed record RetroPayBundleTagsLimitRule(int MaxCount, List<string> Banned) : IRetroPayBundleRule
{
    public List<string> Check(RetroPayBundle item)
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

public sealed record RetroPayBundleCodeLengthRule(int MaxLength) : IRetroPayBundleRule
{
    public List<string> Check(RetroPayBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBundleRequiredCodeRule(string Message) : IRetroPayBundleRule
{
    public List<string> Check(RetroPayBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayBundleRequiredLabelRule(string Message) : IRetroPayBundleRule
{
    public List<string> Check(RetroPayBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay bundle.</summary>
public sealed record RetroPayBundleValidator(List<IRetroPayBundleRule> Rules)
{
    public List<string> Validate(RetroPayBundle item)
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

    public void RequireValid(RetroPayBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayBundle> items)
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

/// <summary>Functions of retro pay bundle rules.</summary>
public static class RetroPayBundleRules
{
    public static RetroPayBundleValidator DefaultRetroPayBundleValidator()
    {
        return new RetroPayBundleValidator(
            new List<IRetroPayBundleRule>
            {
                new RetroPayBundleTagsLimitRule(255, new List<string>()),
                new RetroPayBundleCodeLengthRule(80),
                new RetroPayBundleRequiredCodeRule("retro pay bundle needs a value"),
                new RetroPayBundleRequiredLabelRule("retro pay bundle needs an entry"),
            });
    }
}
