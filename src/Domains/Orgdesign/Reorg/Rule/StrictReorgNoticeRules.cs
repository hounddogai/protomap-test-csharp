namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict reorg notice must pass. Check returns its problems, or none.</summary>
public interface IStrictReorgNoticeRule
{
    List<string> Check(StrictReorgNotice item);
}

public sealed record StrictReorgNoticeRequiredChecksumRule(string Message) : IStrictReorgNoticeRule
{
    public List<string> Check(StrictReorgNotice item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgNoticeCostCenterLengthRule(int MaxLength) : IStrictReorgNoticeRule
{
    public List<string> Check(StrictReorgNotice item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgNoticeAllowedCostCenterRule(List<string> Allowed) : IStrictReorgNoticeRule
{
    public List<string> Check(StrictReorgNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict reorg notice.</summary>
public sealed record StrictReorgNoticeValidator(List<IStrictReorgNoticeRule> Rules)
{
    public List<string> Validate(StrictReorgNotice item)
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

    public void RequireValid(StrictReorgNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReorgNotice> items)
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

/// <summary>Functions of strict reorg notice rules.</summary>
public static class StrictReorgNoticeRules
{
    public static StrictReorgNoticeValidator DefaultStrictReorgNoticeValidator()
    {
        return new StrictReorgNoticeValidator(
            new List<IStrictReorgNoticeRule>
            {
                new StrictReorgNoticeRequiredChecksumRule("strict reorg notice needs this field"),
                new StrictReorgNoticeCostCenterLengthRule(3600),
                new StrictReorgNoticeAllowedCostCenterRule(new List<string> { "fallback" }),
            });
    }
}
