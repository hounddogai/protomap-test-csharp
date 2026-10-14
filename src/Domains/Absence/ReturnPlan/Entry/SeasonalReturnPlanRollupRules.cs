namespace Acme.Workforce.Absence.ReturnPlan.Entry;

/// <summary>A rule a seasonal return plan rollup must pass. Check returns its problems, or none.</summary>
public interface ISeasonalReturnPlanRollupRule
{
    List<string> Check(SeasonalReturnPlanRollup item);
}

public sealed record SeasonalReturnPlanRollupCodeLengthRule(int MaxLength) : ISeasonalReturnPlanRollupRule
{
    public List<string> Check(SeasonalReturnPlanRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReturnPlanRollupAmountRangeRule(double Min, double Max) : ISeasonalReturnPlanRollupRule
{
    public List<string> Check(SeasonalReturnPlanRollup item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReturnPlanRollupRequiredChannelRule(string Message) : ISeasonalReturnPlanRollupRule
{
    public List<string> Check(SeasonalReturnPlanRollup item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReturnPlanRollupWeightRangeRule(double Min, double Max) : ISeasonalReturnPlanRollupRule
{
    public List<string> Check(SeasonalReturnPlanRollup item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalReturnPlanRollupRevisionRangeRule(int Min, int Max) : ISeasonalReturnPlanRollupRule
{
    public List<string> Check(SeasonalReturnPlanRollup item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal return plan rollup.</summary>
public sealed record SeasonalReturnPlanRollupValidator(List<ISeasonalReturnPlanRollupRule> Rules)
{
    public List<string> Validate(SeasonalReturnPlanRollup item)
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

    public void RequireValid(SeasonalReturnPlanRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalReturnPlanRollup> items)
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

/// <summary>Functions of seasonal return plan rollup rules.</summary>
public static class SeasonalReturnPlanRollupRules
{
    public static SeasonalReturnPlanRollupValidator DefaultSeasonalReturnPlanRollupValidator()
    {
        return new SeasonalReturnPlanRollupValidator(
            new List<ISeasonalReturnPlanRollupRule>
            {
                new SeasonalReturnPlanRollupCodeLengthRule(32),
                new SeasonalReturnPlanRollupAmountRangeRule(7.0, 1502.0),
                new SeasonalReturnPlanRollupRequiredChannelRule("seasonal return plan rollup needs a value"),
                new SeasonalReturnPlanRollupWeightRangeRule(2.5, 137.2),
                new SeasonalReturnPlanRollupRevisionRangeRule(120, 3600),
            });
    }
}
