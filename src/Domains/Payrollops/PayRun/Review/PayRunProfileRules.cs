namespace Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>A rule a pay run profile must pass. Check returns its problems, or none.</summary>
public interface IPayRunProfileRule
{
    List<string> Check(PayRunProfile item);
}

public sealed record PayRunProfileCodePrefixRule(string Prefix) : IPayRunProfileRule
{
    public List<string> Check(PayRunProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunProfileRequiredSalaryBandRule(string Message) : IPayRunProfileRule
{
    public List<string> Check(PayRunProfile item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunProfileTagsLimitRule(int MaxCount, List<string> Banned) : IPayRunProfileRule
{
    public List<string> Check(PayRunProfile item)
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

public sealed record PayRunProfileRequiredSkillRule(string Message) : IPayRunProfileRule
{
    public List<string> Check(PayRunProfile item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunProfileRequiredTitleRule(string Message) : IPayRunProfileRule
{
    public List<string> Check(PayRunProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunProfileCodeLengthRule(int MaxLength) : IPayRunProfileRule
{
    public List<string> Check(PayRunProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run profile.</summary>
public sealed record PayRunProfileValidator(List<IPayRunProfileRule> Rules)
{
    public List<string> Validate(PayRunProfile item)
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

    public void RequireValid(PayRunProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunProfile> items)
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

/// <summary>Functions of pay run profile rules.</summary>
public static class PayRunProfileRules
{
    public static PayRunProfileValidator DefaultPayRunProfileValidator()
    {
        return new PayRunProfileValidator(
            new List<IPayRunProfileRule>
            {
                new PayRunProfileCodePrefixRule("Q"),
                new PayRunProfileRequiredSalaryBandRule("pay run profile needs a value"),
                new PayRunProfileTagsLimitRule(64, new List<string> { "strict", "adaptive", "global" }),
                new PayRunProfileRequiredSkillRule("pay run profile needs this field"),
                new PayRunProfileRequiredTitleRule("pay run profile needs an entry"),
                new PayRunProfileCodeLengthRule(255),
            });
    }
}
