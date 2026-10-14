namespace Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>A rule a strict return plan slot must pass. Check returns its problems, or none.</summary>
public interface IStrictReturnPlanSlotRule
{
    List<string> Check(StrictReturnPlanSlot item);
}

public sealed record StrictReturnPlanSlotQuantityRangeRule(int Min, int Max) : IStrictReturnPlanSlotRule
{
    public List<string> Check(StrictReturnPlanSlot item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictReturnPlanSlotCodeLengthRule(int MaxLength) : IStrictReturnPlanSlotRule
{
    public List<string> Check(StrictReturnPlanSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictReturnPlanSlotContactEmailLengthRule(int MaxLength) : IStrictReturnPlanSlotRule
{
    public List<string> Check(StrictReturnPlanSlot item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict return plan slot.</summary>
public sealed record StrictReturnPlanSlotValidator(List<IStrictReturnPlanSlotRule> Rules)
{
    public List<string> Validate(StrictReturnPlanSlot item)
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

    public void RequireValid(StrictReturnPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReturnPlanSlot> items)
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

/// <summary>Functions of strict return plan slot rules.</summary>
public static class StrictReturnPlanSlotRules
{
    public static StrictReturnPlanSlotValidator DefaultStrictReturnPlanSlotValidator()
    {
        return new StrictReturnPlanSlotValidator(
            new List<IStrictReturnPlanSlotRule>
            {
                new StrictReturnPlanSlotQuantityRangeRule(64, 3600),
                new StrictReturnPlanSlotCodeLengthRule(120),
                new StrictReturnPlanSlotContactEmailLengthRule(32),
            });
    }
}
