namespace Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>A rule a manual bonus pool template must pass. Check returns its problems, or none.</summary>
public interface IManualBonusPoolTemplateRule
{
    List<string> Check(ManualBonusPoolTemplate item);
}

public sealed record ManualBonusPoolTemplateNameLengthRule(int MaxLength) : IManualBonusPoolTemplateRule
{
    public List<string> Check(ManualBonusPoolTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolTemplateRequiredTitleRule(string Message) : IManualBonusPoolTemplateRule
{
    public List<string> Check(ManualBonusPoolTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolTemplateTitleLengthRule(int MaxLength) : IManualBonusPoolTemplateRule
{
    public List<string> Check(ManualBonusPoolTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBonusPoolTemplateRequiredCodeRule(string Message) : IManualBonusPoolTemplateRule
{
    public List<string> Check(ManualBonusPoolTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual bonus pool template.</summary>
public sealed record ManualBonusPoolTemplateValidator(List<IManualBonusPoolTemplateRule> Rules)
{
    public List<string> Validate(ManualBonusPoolTemplate item)
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

    public void RequireValid(ManualBonusPoolTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBonusPoolTemplate> items)
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

/// <summary>Functions of manual bonus pool template rules.</summary>
public static class ManualBonusPoolTemplateRules
{
    public static ManualBonusPoolTemplateValidator DefaultManualBonusPoolTemplateValidator()
    {
        return new ManualBonusPoolTemplateValidator(
            new List<IManualBonusPoolTemplateRule>
            {
                new ManualBonusPoolTemplateNameLengthRule(80),
                new ManualBonusPoolTemplateRequiredTitleRule("manual bonus pool template needs this field"),
                new ManualBonusPoolTemplateTitleLengthRule(32),
                new ManualBonusPoolTemplateRequiredCodeRule("manual bonus pool template needs an entry"),
            });
    }
}
