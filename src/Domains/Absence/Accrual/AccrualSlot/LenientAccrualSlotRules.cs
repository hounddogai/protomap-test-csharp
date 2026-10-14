namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

/// <summary>A rule a lenient accrual slot must pass. Check returns its problems, or none.</summary>
public interface ILenientAccrualSlotRule
{
    List<string> Check(LenientAccrualSlot item);
}

public sealed record LenientAccrualSlotLabelLengthRule(int MaxLength) : ILenientAccrualSlotRule
{
    public List<string> Check(LenientAccrualSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualSlotCodeLengthRule(int MaxLength) : ILenientAccrualSlotRule
{
    public List<string> Check(LenientAccrualSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualSlotWindowDaysRangeRule(int Min, int Max) : ILenientAccrualSlotRule
{
    public List<string> Check(LenientAccrualSlot item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualSlotVersionRangeRule(int Min, int Max) : ILenientAccrualSlotRule
{
    public List<string> Check(LenientAccrualSlot item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualSlotRequiredCodeRule(string Message) : ILenientAccrualSlotRule
{
    public List<string> Check(LenientAccrualSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient accrual slot.</summary>
public sealed record LenientAccrualSlotValidator(List<ILenientAccrualSlotRule> Rules)
{
    public List<string> Validate(LenientAccrualSlot item)
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

    public void RequireValid(LenientAccrualSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAccrualSlot> items)
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

/// <summary>Functions of lenient accrual slot rules.</summary>
public static class LenientAccrualSlotRules
{
    public static LenientAccrualSlotValidator DefaultLenientAccrualSlotValidator()
    {
        return new LenientAccrualSlotValidator(
            new List<ILenientAccrualSlotRule>
            {
                new LenientAccrualSlotLabelLengthRule(32),
                new LenientAccrualSlotCodeLengthRule(64),
                new LenientAccrualSlotWindowDaysRangeRule(64, 86400),
                new LenientAccrualSlotVersionRangeRule(1000, 3600),
                new LenientAccrualSlotRequiredCodeRule("lenient accrual slot needs a value"),
            });
    }
}
