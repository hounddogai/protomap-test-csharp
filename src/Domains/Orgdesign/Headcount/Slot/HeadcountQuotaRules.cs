namespace Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>A rule a headcount quota must pass. Check returns its problems, or none.</summary>
public interface IHeadcountQuotaRule
{
    List<string> Check(HeadcountQuota item);
}

public sealed record HeadcountQuotaCodeLengthRule(int MaxLength) : IHeadcountQuotaRule
{
    public List<string> Check(HeadcountQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountQuotaDescriptionLengthRule(int MaxLength) : IHeadcountQuotaRule
{
    public List<string> Check(HeadcountQuota item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountQuotaRequiredDescriptionRule(string Message) : IHeadcountQuotaRule
{
    public List<string> Check(HeadcountQuota item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountQuotaRequiredChannelRule(string Message) : IHeadcountQuotaRule
{
    public List<string> Check(HeadcountQuota item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountQuotaCodePrefixRule(string Prefix) : IHeadcountQuotaRule
{
    public List<string> Check(HeadcountQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount quota.</summary>
public sealed record HeadcountQuotaValidator(List<IHeadcountQuotaRule> Rules)
{
    public List<string> Validate(HeadcountQuota item)
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

    public void RequireValid(HeadcountQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountQuota> items)
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

/// <summary>Functions of headcount quota rules.</summary>
public static class HeadcountQuotaRules
{
    public static HeadcountQuotaValidator DefaultHeadcountQuotaValidator()
    {
        return new HeadcountQuotaValidator(
            new List<IHeadcountQuotaRule>
            {
                new HeadcountQuotaCodeLengthRule(1000),
                new HeadcountQuotaDescriptionLengthRule(1000),
                new HeadcountQuotaRequiredDescriptionRule("headcount quota needs this field"),
                new HeadcountQuotaRequiredChannelRule("headcount quota needs an entry"),
                new HeadcountQuotaCodePrefixRule("REF"),
            });
    }
}
