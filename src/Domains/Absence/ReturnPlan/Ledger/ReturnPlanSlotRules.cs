namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>A rule a return plan slot must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanSlotRule
{
    List<string> Check(ReturnPlanSlot item);
}

public sealed record ReturnPlanSlotUpdatedAtRangeRule(int Min, int Max) : IReturnPlanSlotRule
{
    public List<string> Check(ReturnPlanSlot item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSlotRequiredDescriptionRule(string Message) : IReturnPlanSlotRule
{
    public List<string> Check(ReturnPlanSlot item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSlotArchivedGuardRule(double Limit) : IReturnPlanSlotRule
{
    public List<string> Check(ReturnPlanSlot item)
    {
        if (item.Archived && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSlotLabelLengthRule(int MaxLength) : IReturnPlanSlotRule
{
    public List<string> Check(ReturnPlanSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSlotDescriptionLengthRule(int MaxLength) : IReturnPlanSlotRule
{
    public List<string> Check(ReturnPlanSlot item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSlotHoursRangeRule(double Min, double Max) : IReturnPlanSlotRule
{
    public List<string> Check(ReturnPlanSlot item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan slot.</summary>
public sealed record ReturnPlanSlotValidator(List<IReturnPlanSlotRule> Rules)
{
    public List<string> Validate(ReturnPlanSlot item)
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

    public void RequireValid(ReturnPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanSlot> items)
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

/// <summary>Functions of return plan slot rules.</summary>
public static class ReturnPlanSlotRules
{
    public static ReturnPlanSlotValidator DefaultReturnPlanSlotValidator()
    {
        return new ReturnPlanSlotValidator(
            new List<IReturnPlanSlotRule>
            {
                new ReturnPlanSlotUpdatedAtRangeRule(80, 86400),
                new ReturnPlanSlotRequiredDescriptionRule("return plan slot needs a value"),
                new ReturnPlanSlotArchivedGuardRule(6.2),
                new ReturnPlanSlotLabelLengthRule(86400),
                new ReturnPlanSlotDescriptionLengthRule(255),
                new ReturnPlanSlotHoursRangeRule(1.0, 3975.9),
            });
    }
}
