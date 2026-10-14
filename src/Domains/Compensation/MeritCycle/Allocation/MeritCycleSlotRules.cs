namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>A rule a merit cycle slot must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleSlotRule
{
    List<string> Check(MeritCycleSlot item);
}

public sealed record MeritCycleSlotTitleLengthRule(int MaxLength) : IMeritCycleSlotRule
{
    public List<string> Check(MeritCycleSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSlotUnitLengthRule(int MaxLength) : IMeritCycleSlotRule
{
    public List<string> Check(MeritCycleSlot item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSlotRequiredCodeRule(string Message) : IMeritCycleSlotRule
{
    public List<string> Check(MeritCycleSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleSlotCodeLengthRule(int MaxLength) : IMeritCycleSlotRule
{
    public List<string> Check(MeritCycleSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle slot.</summary>
public sealed record MeritCycleSlotValidator(List<IMeritCycleSlotRule> Rules)
{
    public List<string> Validate(MeritCycleSlot item)
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

    public void RequireValid(MeritCycleSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleSlot> items)
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

/// <summary>Functions of merit cycle slot rules.</summary>
public static class MeritCycleSlotRules
{
    public static MeritCycleSlotValidator DefaultMeritCycleSlotValidator()
    {
        return new MeritCycleSlotValidator(
            new List<IMeritCycleSlotRule>
            {
                new MeritCycleSlotTitleLengthRule(16),
                new MeritCycleSlotUnitLengthRule(16),
                new MeritCycleSlotRequiredCodeRule("merit cycle slot needs this field"),
                new MeritCycleSlotCodeLengthRule(255),
            });
    }
}
