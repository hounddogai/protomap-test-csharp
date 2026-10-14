namespace Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>A rule a rolling adjustment template must pass. Check returns its problems, or none.</summary>
public interface IRollingAdjustmentTemplateRule
{
    List<string> Check(RollingAdjustmentTemplate item);
}

public sealed record RollingAdjustmentTemplateRequiredCurrencyRule(string Message) : IRollingAdjustmentTemplateRule
{
    public List<string> Check(RollingAdjustmentTemplate item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentTemplateRequiredCodeRule(string Message) : IRollingAdjustmentTemplateRule
{
    public List<string> Check(RollingAdjustmentTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentTemplateCodePrefixRule(string Prefix) : IRollingAdjustmentTemplateRule
{
    public List<string> Check(RollingAdjustmentTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentTemplateGraceDaysRangeRule(int Min, int Max) : IRollingAdjustmentTemplateRule
{
    public List<string> Check(RollingAdjustmentTemplate item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentTemplateRequiredTeamRule(string Message) : IRollingAdjustmentTemplateRule
{
    public List<string> Check(RollingAdjustmentTemplate item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentTemplateHoursRangeRule(double Min, double Max) : IRollingAdjustmentTemplateRule
{
    public List<string> Check(RollingAdjustmentTemplate item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling adjustment template.</summary>
public sealed record RollingAdjustmentTemplateValidator(List<IRollingAdjustmentTemplateRule> Rules)
{
    public List<string> Validate(RollingAdjustmentTemplate item)
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

    public void RequireValid(RollingAdjustmentTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAdjustmentTemplate> items)
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

/// <summary>Functions of rolling adjustment template rules.</summary>
public static class RollingAdjustmentTemplateRules
{
    public static RollingAdjustmentTemplateValidator DefaultRollingAdjustmentTemplateValidator()
    {
        return new RollingAdjustmentTemplateValidator(
            new List<IRollingAdjustmentTemplateRule>
            {
                new RollingAdjustmentTemplateRequiredCurrencyRule("rolling adjustment template needs a value"),
                new RollingAdjustmentTemplateRequiredCodeRule("rolling adjustment template needs this field"),
                new RollingAdjustmentTemplateCodePrefixRule("ACM"),
                new RollingAdjustmentTemplateGraceDaysRangeRule(80, 3600),
                new RollingAdjustmentTemplateRequiredTeamRule("rolling adjustment template needs this field"),
                new RollingAdjustmentTemplateHoursRangeRule(4.3, 4193.1),
            });
    }
}
