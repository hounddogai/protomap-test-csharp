namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict accrual template must pass. Check returns its problems, or none.</summary>
public interface IStrictAccrualTemplateRule
{
    List<string> Check(StrictAccrualTemplate item);
}

public sealed record StrictAccrualTemplateAllowedManagerIdRule(List<string> Allowed) : IStrictAccrualTemplateRule
{
    public List<string> Check(StrictAccrualTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualTemplateRequiredNameRule(string Message) : IStrictAccrualTemplateRule
{
    public List<string> Check(StrictAccrualTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualTemplateAttemptsRangeRule(int Min, int Max) : IStrictAccrualTemplateRule
{
    public List<string> Check(StrictAccrualTemplate item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAccrualTemplateMaximumRangeRule(double Min, double Max) : IStrictAccrualTemplateRule
{
    public List<string> Check(StrictAccrualTemplate item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualTemplateRequiredManagerIdRule(string Message) : IStrictAccrualTemplateRule
{
    public List<string> Check(StrictAccrualTemplate item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictAccrualTemplateLabelLengthRule(int MaxLength) : IStrictAccrualTemplateRule
{
    public List<string> Check(StrictAccrualTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict accrual template.</summary>
public sealed record StrictAccrualTemplateValidator(List<IStrictAccrualTemplateRule> Rules)
{
    public List<string> Validate(StrictAccrualTemplate item)
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

    public void RequireValid(StrictAccrualTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccrualTemplate> items)
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

/// <summary>Functions of strict accrual template rules.</summary>
public static class StrictAccrualTemplateRules
{
    public static StrictAccrualTemplateValidator DefaultStrictAccrualTemplateValidator()
    {
        return new StrictAccrualTemplateValidator(
            new List<IStrictAccrualTemplateRule>
            {
                new StrictAccrualTemplateAllowedManagerIdRule(new List<string>()),
                new StrictAccrualTemplateRequiredNameRule("strict accrual template needs this field"),
                new StrictAccrualTemplateAttemptsRangeRule(120, 3600),
                new StrictAccrualTemplateMaximumRangeRule(4.6, 687.6),
                new StrictAccrualTemplateRequiredManagerIdRule("strict accrual template needs an entry"),
                new StrictAccrualTemplateLabelLengthRule(120),
            });
    }
}
