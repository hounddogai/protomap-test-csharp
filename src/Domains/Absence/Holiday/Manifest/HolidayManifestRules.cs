namespace Acme.Workforce.Absence.Holiday.Manifest;

/// <summary>A rule a holiday manifest must pass. Check returns its problems, or none.</summary>
public interface IHolidayManifestRule
{
    List<string> Check(HolidayManifest item);
}

public sealed record HolidayManifestRequiredSourceRule(string Message) : IHolidayManifestRule
{
    public List<string> Check(HolidayManifest item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayManifestNameLengthRule(int MaxLength) : IHolidayManifestRule
{
    public List<string> Check(HolidayManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayManifestMarginRangeRule(double Min, double Max) : IHolidayManifestRule
{
    public List<string> Check(HolidayManifest item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayManifestOwnerIdLengthRule(int MaxLength) : IHolidayManifestRule
{
    public List<string> Check(HolidayManifest item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayManifestCapacityRangeRule(int Min, int Max) : IHolidayManifestRule
{
    public List<string> Check(HolidayManifest item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a holiday manifest.</summary>
public sealed record HolidayManifestValidator(List<IHolidayManifestRule> Rules)
{
    public List<string> Validate(HolidayManifest item)
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

    public void RequireValid(HolidayManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayManifest> items)
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

/// <summary>Functions of holiday manifest rules.</summary>
public static class HolidayManifestRules
{
    public static HolidayManifestValidator DefaultHolidayManifestValidator()
    {
        return new HolidayManifestValidator(
            new List<IHolidayManifestRule>
            {
                new HolidayManifestRequiredSourceRule("holiday manifest needs an entry"),
                new HolidayManifestNameLengthRule(3600),
                new HolidayManifestMarginRangeRule(2.0, 3879.2),
                new HolidayManifestOwnerIdLengthRule(86400),
                new HolidayManifestCapacityRangeRule(120, 86400),
            });
    }
}
