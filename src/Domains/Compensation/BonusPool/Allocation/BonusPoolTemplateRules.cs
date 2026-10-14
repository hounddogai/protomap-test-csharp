namespace Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>A rule a bonus pool template must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolTemplateRule
{
    List<string> Check(BonusPoolTemplate item);
}

public sealed record BonusPoolTemplateRequiredDepartmentRule(string Message) : IBonusPoolTemplateRule
{
    public List<string> Check(BonusPoolTemplate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolTemplateCodePrefixRule(string Prefix) : IBonusPoolTemplateRule
{
    public List<string> Check(BonusPoolTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolTemplateTitleLengthRule(int MaxLength) : IBonusPoolTemplateRule
{
    public List<string> Check(BonusPoolTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolTemplateQuantityRangeRule(int Min, int Max) : IBonusPoolTemplateRule
{
    public List<string> Check(BonusPoolTemplate item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool template.</summary>
public sealed record BonusPoolTemplateValidator(List<IBonusPoolTemplateRule> Rules)
{
    public List<string> Validate(BonusPoolTemplate item)
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

    public void RequireValid(BonusPoolTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolTemplate> items)
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

/// <summary>Functions of bonus pool template rules.</summary>
public static class BonusPoolTemplateRules
{
    public static BonusPoolTemplateValidator DefaultBonusPoolTemplateValidator()
    {
        return new BonusPoolTemplateValidator(
            new List<IBonusPoolTemplateRule>
            {
                new BonusPoolTemplateRequiredDepartmentRule("bonus pool template needs this field"),
                new BonusPoolTemplateCodePrefixRule("REF"),
                new BonusPoolTemplateTitleLengthRule(16),
                new BonusPoolTemplateQuantityRangeRule(32, 3600),
            });
    }
}
