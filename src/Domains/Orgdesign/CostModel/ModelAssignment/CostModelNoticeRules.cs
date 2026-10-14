namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model notice must pass. Check returns its problems, or none.</summary>
public interface ICostModelNoticeRule
{
    List<string> Check(CostModelNotice item);
}

public sealed record CostModelNoticeRequiredCodeRule(string Message) : ICostModelNoticeRule
{
    public List<string> Check(CostModelNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelNoticeTitleLengthRule(int MaxLength) : ICostModelNoticeRule
{
    public List<string> Check(CostModelNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelNoticeRequiredLocationRule(string Message) : ICostModelNoticeRule
{
    public List<string> Check(CostModelNotice item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelNoticeRequiredRegionRule(string Message) : ICostModelNoticeRule
{
    public List<string> Check(CostModelNotice item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelNoticeCodePrefixRule(string Prefix) : ICostModelNoticeRule
{
    public List<string> Check(CostModelNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelNoticeAllowedRegionRule(List<string> Allowed) : ICostModelNoticeRule
{
    public List<string> Check(CostModelNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model notice.</summary>
public sealed record CostModelNoticeValidator(List<ICostModelNoticeRule> Rules)
{
    public List<string> Validate(CostModelNotice item)
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

    public void RequireValid(CostModelNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelNotice> items)
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

/// <summary>Functions of cost model notice rules.</summary>
public static class CostModelNoticeRules
{
    public static CostModelNoticeValidator DefaultCostModelNoticeValidator()
    {
        return new CostModelNoticeValidator(
            new List<ICostModelNoticeRule>
            {
                new CostModelNoticeRequiredCodeRule("cost model notice needs a value"),
                new CostModelNoticeTitleLengthRule(16),
                new CostModelNoticeRequiredLocationRule("cost model notice needs this field"),
                new CostModelNoticeRequiredRegionRule("cost model notice needs a value"),
                new CostModelNoticeCodePrefixRule("X"),
                new CostModelNoticeAllowedRegionRule(new List<string>()),
            });
    }
}
