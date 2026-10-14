namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled tax form template must pass. Check returns its problems, or none.</summary>
public interface IScheduledTaxFormTemplateRule
{
    List<string> Check(ScheduledTaxFormTemplate item);
}

public sealed record ScheduledTaxFormTemplateRequiredNameRule(string Message) : IScheduledTaxFormTemplateRule
{
    public List<string> Check(ScheduledTaxFormTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormTemplateRequiredSkillRule(string Message) : IScheduledTaxFormTemplateRule
{
    public List<string> Check(ScheduledTaxFormTemplate item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTaxFormTemplateAllowedNameRule(List<string> Allowed) : IScheduledTaxFormTemplateRule
{
    public List<string> Check(ScheduledTaxFormTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled tax form template.</summary>
public sealed record ScheduledTaxFormTemplateValidator(List<IScheduledTaxFormTemplateRule> Rules)
{
    public List<string> Validate(ScheduledTaxFormTemplate item)
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

    public void RequireValid(ScheduledTaxFormTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTaxFormTemplate> items)
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

/// <summary>Functions of scheduled tax form template rules.</summary>
public static class ScheduledTaxFormTemplateRules
{
    public static ScheduledTaxFormTemplateValidator DefaultScheduledTaxFormTemplateValidator()
    {
        return new ScheduledTaxFormTemplateValidator(
            new List<IScheduledTaxFormTemplateRule>
            {
                new ScheduledTaxFormTemplateRequiredNameRule("scheduled tax form template needs a value"),
                new ScheduledTaxFormTemplateRequiredSkillRule("scheduled tax form template needs an entry"),
                new ScheduledTaxFormTemplateAllowedNameRule(new List<string> { "fallback", "batched" }),
            });
    }
}
