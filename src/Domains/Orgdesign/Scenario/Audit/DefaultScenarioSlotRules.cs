namespace Acme.Workforce.Orgdesign.Scenario.Audit;

/// <summary>A rule a default scenario slot must pass. Check returns its problems, or none.</summary>
public interface IDefaultScenarioSlotRule
{
    List<string> Check(DefaultScenarioSlot item);
}

public sealed record DefaultScenarioSlotWeightRangeRule(double Min, double Max) : IDefaultScenarioSlotRule
{
    public List<string> Check(DefaultScenarioSlot item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultScenarioSlotUnitLengthRule(int MaxLength) : IDefaultScenarioSlotRule
{
    public List<string> Check(DefaultScenarioSlot item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultScenarioSlotRequiredCodeRule(string Message) : IDefaultScenarioSlotRule
{
    public List<string> Check(DefaultScenarioSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultScenarioSlotTitleLengthRule(int MaxLength) : IDefaultScenarioSlotRule
{
    public List<string> Check(DefaultScenarioSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultScenarioSlotRequiredUnitRule(string Message) : IDefaultScenarioSlotRule
{
    public List<string> Check(DefaultScenarioSlot item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default scenario slot.</summary>
public sealed record DefaultScenarioSlotValidator(List<IDefaultScenarioSlotRule> Rules)
{
    public List<string> Validate(DefaultScenarioSlot item)
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

    public void RequireValid(DefaultScenarioSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultScenarioSlot> items)
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

/// <summary>Functions of default scenario slot rules.</summary>
public static class DefaultScenarioSlotRules
{
    public static DefaultScenarioSlotValidator DefaultDefaultScenarioSlotValidator()
    {
        return new DefaultScenarioSlotValidator(
            new List<IDefaultScenarioSlotRule>
            {
                new DefaultScenarioSlotWeightRangeRule(9.4, 906.5),
                new DefaultScenarioSlotUnitLengthRule(255),
                new DefaultScenarioSlotRequiredCodeRule("default scenario slot needs an entry"),
                new DefaultScenarioSlotTitleLengthRule(3600),
                new DefaultScenarioSlotRequiredUnitRule("default scenario slot needs a value"),
            });
    }
}
