namespace Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>A rule a global scenario slot must pass. Check returns its problems, or none.</summary>
public interface IGlobalScenarioSlotRule
{
    List<string> Check(GlobalScenarioSlot item);
}

public sealed record GlobalScenarioSlotExternalRefLengthRule(int MaxLength) : IGlobalScenarioSlotRule
{
    public List<string> Check(GlobalScenarioSlot item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioSlotPinnedGuardRule(int Limit) : IGlobalScenarioSlotRule
{
    public List<string> Check(GlobalScenarioSlot item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioSlotCodePrefixRule(string Prefix) : IGlobalScenarioSlotRule
{
    public List<string> Check(GlobalScenarioSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioSlotRequiredLabelRule(string Message) : IGlobalScenarioSlotRule
{
    public List<string> Check(GlobalScenarioSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioSlotRequiredExternalRefRule(string Message) : IGlobalScenarioSlotRule
{
    public List<string> Check(GlobalScenarioSlot item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalScenarioSlotRequiredPhoneRule(string Message) : IGlobalScenarioSlotRule
{
    public List<string> Check(GlobalScenarioSlot item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global scenario slot.</summary>
public sealed record GlobalScenarioSlotValidator(List<IGlobalScenarioSlotRule> Rules)
{
    public List<string> Validate(GlobalScenarioSlot item)
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

    public void RequireValid(GlobalScenarioSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalScenarioSlot> items)
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

/// <summary>Functions of global scenario slot rules.</summary>
public static class GlobalScenarioSlotRules
{
    public static GlobalScenarioSlotValidator DefaultGlobalScenarioSlotValidator()
    {
        return new GlobalScenarioSlotValidator(
            new List<IGlobalScenarioSlotRule>
            {
                new GlobalScenarioSlotExternalRefLengthRule(80),
                new GlobalScenarioSlotPinnedGuardRule(120),
                new GlobalScenarioSlotCodePrefixRule("X"),
                new GlobalScenarioSlotRequiredLabelRule("global scenario slot needs a value"),
                new GlobalScenarioSlotRequiredExternalRefRule("global scenario slot needs a value"),
                new GlobalScenarioSlotRequiredPhoneRule("global scenario slot needs this field"),
            });
    }
}
