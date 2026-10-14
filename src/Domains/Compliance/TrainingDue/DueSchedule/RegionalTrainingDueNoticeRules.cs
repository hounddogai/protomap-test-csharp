namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional training due notice must pass. Check returns its problems, or none.</summary>
public interface IRegionalTrainingDueNoticeRule
{
    List<string> Check(RegionalTrainingDueNotice item);
}

public sealed record RegionalTrainingDueNoticeCodePrefixRule(string Prefix) : IRegionalTrainingDueNoticeRule
{
    public List<string> Check(RegionalTrainingDueNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueNoticeAllowedCostCenterRule(List<string> Allowed) : IRegionalTrainingDueNoticeRule
{
    public List<string> Check(RegionalTrainingDueNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTrainingDueNoticeLabelLengthRule(int MaxLength) : IRegionalTrainingDueNoticeRule
{
    public List<string> Check(RegionalTrainingDueNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional training due notice.</summary>
public sealed record RegionalTrainingDueNoticeValidator(List<IRegionalTrainingDueNoticeRule> Rules)
{
    public List<string> Validate(RegionalTrainingDueNotice item)
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

    public void RequireValid(RegionalTrainingDueNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTrainingDueNotice> items)
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

/// <summary>Functions of regional training due notice rules.</summary>
public static class RegionalTrainingDueNoticeRules
{
    public static RegionalTrainingDueNoticeValidator DefaultRegionalTrainingDueNoticeValidator()
    {
        return new RegionalTrainingDueNoticeValidator(
            new List<IRegionalTrainingDueNoticeRule>
            {
                new RegionalTrainingDueNoticeCodePrefixRule("Q"),
                new RegionalTrainingDueNoticeAllowedCostCenterRule(new List<string> { "fallback", "primary" }),
                new RegionalTrainingDueNoticeLabelLengthRule(86400),
            });
    }
}
