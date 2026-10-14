namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled position template must pass. Check returns its problems, or none.</summary>
public interface IScheduledPositionTemplateRule
{
    List<string> Check(ScheduledPositionTemplate item);
}

public sealed record ScheduledPositionTemplateRequiredTitleRule(string Message) : IScheduledPositionTemplateRule
{
    public List<string> Check(ScheduledPositionTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPositionTemplateRequiredSummaryRule(string Message) : IScheduledPositionTemplateRule
{
    public List<string> Check(ScheduledPositionTemplate item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPositionTemplateAllowedTitleRule(List<string> Allowed) : IScheduledPositionTemplateRule
{
    public List<string> Check(ScheduledPositionTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPositionTemplateRequiredUnitRule(string Message) : IScheduledPositionTemplateRule
{
    public List<string> Check(ScheduledPositionTemplate item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPositionTemplateUnitLengthRule(int MaxLength) : IScheduledPositionTemplateRule
{
    public List<string> Check(ScheduledPositionTemplate item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPositionTemplateCodeLengthRule(int MaxLength) : IScheduledPositionTemplateRule
{
    public List<string> Check(ScheduledPositionTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled position template.</summary>
public sealed record ScheduledPositionTemplateValidator(List<IScheduledPositionTemplateRule> Rules)
{
    public List<string> Validate(ScheduledPositionTemplate item)
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

    public void RequireValid(ScheduledPositionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPositionTemplate> items)
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

/// <summary>Functions of scheduled position template rules.</summary>
public static class ScheduledPositionTemplateRules
{
    public static ScheduledPositionTemplateValidator DefaultScheduledPositionTemplateValidator()
    {
        return new ScheduledPositionTemplateValidator(
            new List<IScheduledPositionTemplateRule>
            {
                new ScheduledPositionTemplateRequiredTitleRule("scheduled position template needs a value"),
                new ScheduledPositionTemplateRequiredSummaryRule("scheduled position template needs this field"),
                new ScheduledPositionTemplateAllowedTitleRule(new List<string> { "capped", "rolling" }),
                new ScheduledPositionTemplateRequiredUnitRule("scheduled position template needs a value"),
                new ScheduledPositionTemplateUnitLengthRule(86400),
                new ScheduledPositionTemplateCodeLengthRule(16),
            });
    }
}
