namespace Acme.Workforce.Absence.Accrual.Slot;

/// <summary>A rule a accrual slot must pass. Check returns its problems, or none.</summary>
public interface IAccrualSlotRule
{
    List<string> Check(AccrualSlot item);
}

public sealed record AccrualSlotRequiredCodeRule(string Message) : IAccrualSlotRule
{
    public List<string> Check(AccrualSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualSlotRequiredRegionRule(string Message) : IAccrualSlotRule
{
    public List<string> Check(AccrualSlot item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualSlotCodeLengthRule(int MaxLength) : IAccrualSlotRule
{
    public List<string> Check(AccrualSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSlotRequiredNameRule(string Message) : IAccrualSlotRule
{
    public List<string> Check(AccrualSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualSlotNameLengthRule(int MaxLength) : IAccrualSlotRule
{
    public List<string> Check(AccrualSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSlotMinimumRangeRule(double Min, double Max) : IAccrualSlotRule
{
    public List<string> Check(AccrualSlot item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a accrual slot.</summary>
public sealed record AccrualSlotValidator(List<IAccrualSlotRule> Rules)
{
    public List<string> Validate(AccrualSlot item)
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

    public void RequireValid(AccrualSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualSlot> items)
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

/// <summary>Functions of accrual slot rules.</summary>
public static class AccrualSlotRules
{
    public static AccrualSlotValidator DefaultAccrualSlotValidator()
    {
        return new AccrualSlotValidator(
            new List<IAccrualSlotRule>
            {
                new AccrualSlotRequiredCodeRule("accrual slot needs this field"),
                new AccrualSlotRequiredRegionRule("accrual slot needs a value"),
                new AccrualSlotCodeLengthRule(3600),
                new AccrualSlotRequiredNameRule("accrual slot needs this field"),
                new AccrualSlotNameLengthRule(64),
                new AccrualSlotMinimumRangeRule(8.9, 2221.4),
            });
    }
}
