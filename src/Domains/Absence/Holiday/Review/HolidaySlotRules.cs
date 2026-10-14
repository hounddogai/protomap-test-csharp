namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>A rule a holiday slot must pass. Check returns its problems, or none.</summary>
public interface IHolidaySlotRule
{
    List<string> Check(HolidaySlot item);
}

public sealed record HolidaySlotRequiredDescriptionRule(string Message) : IHolidaySlotRule
{
    public List<string> Check(HolidaySlot item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidaySlotCodeLengthRule(int MaxLength) : IHolidaySlotRule
{
    public List<string> Check(HolidaySlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidaySlotTitleLengthRule(int MaxLength) : IHolidaySlotRule
{
    public List<string> Check(HolidaySlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday slot.</summary>
public sealed record HolidaySlotValidator(List<IHolidaySlotRule> Rules)
{
    public List<string> Validate(HolidaySlot item)
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

    public void RequireValid(HolidaySlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidaySlot> items)
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

/// <summary>Functions of holiday slot rules.</summary>
public static class HolidaySlotRules
{
    public static HolidaySlotValidator DefaultHolidaySlotValidator()
    {
        return new HolidaySlotValidator(
            new List<IHolidaySlotRule>
            {
                new HolidaySlotRequiredDescriptionRule("holiday slot needs a value"),
                new HolidaySlotCodeLengthRule(3600),
                new HolidaySlotTitleLengthRule(120),
            });
    }
}
