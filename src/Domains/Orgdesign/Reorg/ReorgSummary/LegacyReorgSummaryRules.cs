namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy reorg summary must pass. Check returns its problems, or none.</summary>
public interface ILegacyReorgSummaryRule
{
    List<string> Check(LegacyReorgSummary item);
}

public sealed record LegacyReorgSummaryAllowedTitleRule(List<string> Allowed) : ILegacyReorgSummaryRule
{
    public List<string> Check(LegacyReorgSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LegacyReorgSummaryRequiredTitleRule(string Message) : ILegacyReorgSummaryRule
{
    public List<string> Check(LegacyReorgSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyReorgSummaryRequiredBadgeRule(string Message) : ILegacyReorgSummaryRule
{
    public List<string> Check(LegacyReorgSummary item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyReorgSummaryRequiredChannelRule(string Message) : ILegacyReorgSummaryRule
{
    public List<string> Check(LegacyReorgSummary item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyReorgSummaryTitleLengthRule(int MaxLength) : ILegacyReorgSummaryRule
{
    public List<string> Check(LegacyReorgSummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyReorgSummaryChannelLengthRule(int MaxLength) : ILegacyReorgSummaryRule
{
    public List<string> Check(LegacyReorgSummary item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy reorg summary.</summary>
public sealed record LegacyReorgSummaryValidator(List<ILegacyReorgSummaryRule> Rules)
{
    public List<string> Validate(LegacyReorgSummary item)
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

    public void RequireValid(LegacyReorgSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyReorgSummary> items)
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

/// <summary>Functions of legacy reorg summary rules.</summary>
public static class LegacyReorgSummaryRules
{
    public static LegacyReorgSummaryValidator DefaultLegacyReorgSummaryValidator()
    {
        return new LegacyReorgSummaryValidator(
            new List<ILegacyReorgSummaryRule>
            {
                new LegacyReorgSummaryAllowedTitleRule(new List<string> { "regional", "primary", "lenient" }),
                new LegacyReorgSummaryRequiredTitleRule("legacy reorg summary needs this field"),
                new LegacyReorgSummaryRequiredBadgeRule("legacy reorg summary needs this field"),
                new LegacyReorgSummaryRequiredChannelRule("legacy reorg summary needs this field"),
                new LegacyReorgSummaryTitleLengthRule(1000),
                new LegacyReorgSummaryChannelLengthRule(3600),
            });
    }
}
