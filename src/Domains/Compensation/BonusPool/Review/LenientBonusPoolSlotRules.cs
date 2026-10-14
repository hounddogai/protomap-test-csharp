namespace Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>A rule a lenient bonus pool slot must pass. Check returns its problems, or none.</summary>
public interface ILenientBonusPoolSlotRule
{
    List<string> Check(LenientBonusPoolSlot item);
}

public sealed record LenientBonusPoolSlotRequiredGradeRule(string Message) : ILenientBonusPoolSlotRule
{
    public List<string> Check(LenientBonusPoolSlot item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolSlotDepartmentLengthRule(int MaxLength) : ILenientBonusPoolSlotRule
{
    public List<string> Check(LenientBonusPoolSlot item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolSlotRequiredCodeRule(string Message) : ILenientBonusPoolSlotRule
{
    public List<string> Check(LenientBonusPoolSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBonusPoolSlotFloorValueRangeRule(double Min, double Max) : ILenientBonusPoolSlotRule
{
    public List<string> Check(LenientBonusPoolSlot item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientBonusPoolSlotRevisionRangeRule(int Min, int Max) : ILenientBonusPoolSlotRule
{
    public List<string> Check(LenientBonusPoolSlot item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient bonus pool slot.</summary>
public sealed record LenientBonusPoolSlotValidator(List<ILenientBonusPoolSlotRule> Rules)
{
    public List<string> Validate(LenientBonusPoolSlot item)
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

    public void RequireValid(LenientBonusPoolSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBonusPoolSlot> items)
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

/// <summary>Functions of lenient bonus pool slot rules.</summary>
public static class LenientBonusPoolSlotRules
{
    public static LenientBonusPoolSlotValidator DefaultLenientBonusPoolSlotValidator()
    {
        return new LenientBonusPoolSlotValidator(
            new List<ILenientBonusPoolSlotRule>
            {
                new LenientBonusPoolSlotRequiredGradeRule("lenient bonus pool slot needs an entry"),
                new LenientBonusPoolSlotDepartmentLengthRule(120),
                new LenientBonusPoolSlotRequiredCodeRule("lenient bonus pool slot needs a value"),
                new LenientBonusPoolSlotFloorValueRangeRule(8.2, 3429.7),
                new LenientBonusPoolSlotRevisionRangeRule(120, 120),
            });
    }
}
