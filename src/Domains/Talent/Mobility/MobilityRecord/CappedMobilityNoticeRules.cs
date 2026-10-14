namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped mobility notice must pass. Check returns its problems, or none.</summary>
public interface ICappedMobilityNoticeRule
{
    List<string> Check(CappedMobilityNotice item);
}

public sealed record CappedMobilityNoticeAllowedCategoryRule(List<string> Allowed) : ICappedMobilityNoticeRule
{
    public List<string> Check(CappedMobilityNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record CappedMobilityNoticeCodePrefixRule(string Prefix) : ICappedMobilityNoticeRule
{
    public List<string> Check(CappedMobilityNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedMobilityNoticeRequiredCategoryRule(string Message) : ICappedMobilityNoticeRule
{
    public List<string> Check(CappedMobilityNotice item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped mobility notice.</summary>
public sealed record CappedMobilityNoticeValidator(List<ICappedMobilityNoticeRule> Rules)
{
    public List<string> Validate(CappedMobilityNotice item)
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

    public void RequireValid(CappedMobilityNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedMobilityNotice> items)
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

/// <summary>Functions of capped mobility notice rules.</summary>
public static class CappedMobilityNoticeRules
{
    public static CappedMobilityNoticeValidator DefaultCappedMobilityNoticeValidator()
    {
        return new CappedMobilityNoticeValidator(
            new List<ICappedMobilityNoticeRule>
            {
                new CappedMobilityNoticeAllowedCategoryRule(new List<string> { "primary", "capped", "scheduled" }),
                new CappedMobilityNoticeCodePrefixRule("ACM"),
                new CappedMobilityNoticeRequiredCategoryRule("capped mobility notice needs this field"),
            });
    }
}
