namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment template must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentTemplateRule
{
    List<string> Check(GarnishmentTemplate item);
}

public sealed record GarnishmentTemplateDiscountRangeRule(double Min, double Max) : IGarnishmentTemplateRule
{
    public List<string> Check(GarnishmentTemplate item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTemplateRequiredCodeRule(string Message) : IGarnishmentTemplateRule
{
    public List<string> Check(GarnishmentTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTemplateAllowedBadgeRule(List<string> Allowed) : IGarnishmentTemplateRule
{
    public List<string> Check(GarnishmentTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTemplatePriorityRangeRule(int Min, int Max) : IGarnishmentTemplateRule
{
    public List<string> Check(GarnishmentTemplate item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTemplateLabelLengthRule(int MaxLength) : IGarnishmentTemplateRule
{
    public List<string> Check(GarnishmentTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTemplateExternalRefLengthRule(int MaxLength) : IGarnishmentTemplateRule
{
    public List<string> Check(GarnishmentTemplate item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment template.</summary>
public sealed record GarnishmentTemplateValidator(List<IGarnishmentTemplateRule> Rules)
{
    public List<string> Validate(GarnishmentTemplate item)
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

    public void RequireValid(GarnishmentTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentTemplate> items)
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

/// <summary>Functions of garnishment template rules.</summary>
public static class GarnishmentTemplateRules
{
    public static GarnishmentTemplateValidator DefaultGarnishmentTemplateValidator()
    {
        return new GarnishmentTemplateValidator(
            new List<IGarnishmentTemplateRule>
            {
                new GarnishmentTemplateDiscountRangeRule(8.8, 128.9),
                new GarnishmentTemplateRequiredCodeRule("garnishment template needs an entry"),
                new GarnishmentTemplateAllowedBadgeRule(new List<string> { "capped", "weighted", "regional" }),
                new GarnishmentTemplatePriorityRangeRule(32, 80),
                new GarnishmentTemplateLabelLengthRule(3600),
                new GarnishmentTemplateExternalRefLengthRule(255),
            });
    }
}
