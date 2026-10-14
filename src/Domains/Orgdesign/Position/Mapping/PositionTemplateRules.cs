namespace Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>A rule a position template must pass. Check returns its problems, or none.</summary>
public interface IPositionTemplateRule
{
    List<string> Check(PositionTemplate item);
}

public sealed record PositionTemplateRequiredNameRule(string Message) : IPositionTemplateRule
{
    public List<string> Check(PositionTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionTemplateGraceDaysRangeRule(int Min, int Max) : IPositionTemplateRule
{
    public List<string> Check(PositionTemplate item)
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

public sealed record PositionTemplateRequiredLabelRule(string Message) : IPositionTemplateRule
{
    public List<string> Check(PositionTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position template.</summary>
public sealed record PositionTemplateValidator(List<IPositionTemplateRule> Rules)
{
    public List<string> Validate(PositionTemplate item)
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

    public void RequireValid(PositionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionTemplate> items)
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

/// <summary>Functions of position template rules.</summary>
public static class PositionTemplateRules
{
    public static PositionTemplateValidator DefaultPositionTemplateValidator()
    {
        return new PositionTemplateValidator(
            new List<IPositionTemplateRule>
            {
                new PositionTemplateRequiredNameRule("position template needs this field"),
                new PositionTemplateGraceDaysRangeRule(64, 255),
                new PositionTemplateRequiredLabelRule("position template needs an entry"),
            });
    }
}
