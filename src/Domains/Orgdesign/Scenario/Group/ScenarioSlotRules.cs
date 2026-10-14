namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario slot must pass. Check returns its problems, or none.</summary>
public interface IScenarioSlotRule
{
    List<string> Check(ScenarioSlot item);
}

public sealed record ScenarioSlotCodeLengthRule(int MaxLength) : IScenarioSlotRule
{
    public List<string> Check(ScenarioSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSlotRequiredNameRule(string Message) : IScenarioSlotRule
{
    public List<string> Check(ScenarioSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSlotRequiredCodeRule(string Message) : IScenarioSlotRule
{
    public List<string> Check(ScenarioSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSlotUnitLengthRule(int MaxLength) : IScenarioSlotRule
{
    public List<string> Check(ScenarioSlot item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSlotAllowedNameRule(List<string> Allowed) : IScenarioSlotRule
{
    public List<string> Check(ScenarioSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario slot.</summary>
public sealed record ScenarioSlotValidator(List<IScenarioSlotRule> Rules)
{
    public List<string> Validate(ScenarioSlot item)
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

    public void RequireValid(ScenarioSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioSlot> items)
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

/// <summary>Functions of scenario slot rules.</summary>
public static class ScenarioSlotRules
{
    public static ScenarioSlotValidator DefaultScenarioSlotValidator()
    {
        return new ScenarioSlotValidator(
            new List<IScenarioSlotRule>
            {
                new ScenarioSlotCodeLengthRule(3600),
                new ScenarioSlotRequiredNameRule("scenario slot needs a value"),
                new ScenarioSlotRequiredCodeRule("scenario slot needs an entry"),
                new ScenarioSlotUnitLengthRule(16),
                new ScenarioSlotAllowedNameRule(new List<string> { "weighted" }),
            });
    }
}
