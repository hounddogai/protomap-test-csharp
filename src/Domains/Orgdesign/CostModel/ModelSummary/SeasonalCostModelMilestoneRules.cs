namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal cost model milestone must pass. Check returns its problems, or none.</summary>
public interface ISeasonalCostModelMilestoneRule
{
    List<string> Check(SeasonalCostModelMilestone item);
}

public sealed record SeasonalCostModelMilestoneRequiredCodeRule(string Message) : ISeasonalCostModelMilestoneRule
{
    public List<string> Check(SeasonalCostModelMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelMilestoneGraceDaysRangeRule(int Min, int Max) : ISeasonalCostModelMilestoneRule
{
    public List<string> Check(SeasonalCostModelMilestone item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalCostModelMilestoneAllowedCodeRule(List<string> Allowed) : ISeasonalCostModelMilestoneRule
{
    public List<string> Check(SeasonalCostModelMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalCostModelMilestoneCodePrefixRule(string Prefix) : ISeasonalCostModelMilestoneRule
{
    public List<string> Check(SeasonalCostModelMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal cost model milestone.</summary>
public sealed record SeasonalCostModelMilestoneValidator(List<ISeasonalCostModelMilestoneRule> Rules)
{
    public List<string> Validate(SeasonalCostModelMilestone item)
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

    public void RequireValid(SeasonalCostModelMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalCostModelMilestone> items)
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

/// <summary>Functions of seasonal cost model milestone rules.</summary>
public static class SeasonalCostModelMilestoneRules
{
    public static SeasonalCostModelMilestoneValidator DefaultSeasonalCostModelMilestoneValidator()
    {
        return new SeasonalCostModelMilestoneValidator(
            new List<ISeasonalCostModelMilestoneRule>
            {
                new SeasonalCostModelMilestoneRequiredCodeRule("seasonal cost model milestone needs an entry"),
                new SeasonalCostModelMilestoneGraceDaysRangeRule(64, 3600),
                new SeasonalCostModelMilestoneAllowedCodeRule(new List<string> { "global", "pending" }),
                new SeasonalCostModelMilestoneCodePrefixRule("REF"),
            });
    }
}
