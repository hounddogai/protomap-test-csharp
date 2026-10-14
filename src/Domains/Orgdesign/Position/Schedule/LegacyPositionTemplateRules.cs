namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy position template must pass. Check returns its problems, or none.</summary>
public interface ILegacyPositionTemplateRule
{
    List<string> Check(LegacyPositionTemplate item);
}

public sealed record LegacyPositionTemplateAllowedGradeRule(List<string> Allowed) : ILegacyPositionTemplateRule
{
    public List<string> Check(LegacyPositionTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionTemplateDurationMinutesRangeRule(int Min, int Max) : ILegacyPositionTemplateRule
{
    public List<string> Check(LegacyPositionTemplate item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPositionTemplateThresholdRangeRule(double Min, double Max) : ILegacyPositionTemplateRule
{
    public List<string> Check(LegacyPositionTemplate item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy position template.</summary>
public sealed record LegacyPositionTemplateValidator(List<ILegacyPositionTemplateRule> Rules)
{
    public List<string> Validate(LegacyPositionTemplate item)
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

    public void RequireValid(LegacyPositionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPositionTemplate> items)
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

/// <summary>Functions of legacy position template rules.</summary>
public static class LegacyPositionTemplateRules
{
    public static LegacyPositionTemplateValidator DefaultLegacyPositionTemplateValidator()
    {
        return new LegacyPositionTemplateValidator(
            new List<ILegacyPositionTemplateRule>
            {
                new LegacyPositionTemplateAllowedGradeRule(new List<string> { "capped", "regional" }),
                new LegacyPositionTemplateDurationMinutesRangeRule(16, 64),
                new LegacyPositionTemplateThresholdRangeRule(6.3, 286.5),
            });
    }
}
