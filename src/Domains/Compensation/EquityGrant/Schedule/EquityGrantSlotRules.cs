namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>A rule a equity grant slot must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantSlotRule
{
    List<string> Check(EquityGrantSlot item);
}

public sealed record EquityGrantSlotCreatedAtRangeRule(int Min, int Max) : IEquityGrantSlotRule
{
    public List<string> Check(EquityGrantSlot item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSlotRequiredLabelRule(string Message) : IEquityGrantSlotRule
{
    public List<string> Check(EquityGrantSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantSlotRequiredCodeRule(string Message) : IEquityGrantSlotRule
{
    public List<string> Check(EquityGrantSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant slot.</summary>
public sealed record EquityGrantSlotValidator(List<IEquityGrantSlotRule> Rules)
{
    public List<string> Validate(EquityGrantSlot item)
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

    public void RequireValid(EquityGrantSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantSlot> items)
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

/// <summary>Functions of equity grant slot rules.</summary>
public static class EquityGrantSlotRules
{
    public static EquityGrantSlotValidator DefaultEquityGrantSlotValidator()
    {
        return new EquityGrantSlotValidator(
            new List<IEquityGrantSlotRule>
            {
                new EquityGrantSlotCreatedAtRangeRule(64, 86400),
                new EquityGrantSlotRequiredLabelRule("equity grant slot needs this field"),
                new EquityGrantSlotRequiredCodeRule("equity grant slot needs this field"),
            });
    }
}
