namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday request must pass. Check returns its problems, or none.</summary>
public interface IHolidayRequestRule
{
    List<string> Check(HolidayRequest item);
}

public sealed record HolidayRequestNameLengthRule(int MaxLength) : IHolidayRequestRule
{
    public List<string> Check(HolidayRequest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayRequestRequiredJobCodeRule(string Message) : IHolidayRequestRule
{
    public List<string> Check(HolidayRequest item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayRequestRevisionRangeRule(int Min, int Max) : IHolidayRequestRule
{
    public List<string> Check(HolidayRequest item)
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

public sealed record HolidayRequestCodeLengthRule(int MaxLength) : IHolidayRequestRule
{
    public List<string> Check(HolidayRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayRequestPriorityRangeRule(int Min, int Max) : IHolidayRequestRule
{
    public List<string> Check(HolidayRequest item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayRequestAllowedNameRule(List<string> Allowed) : IHolidayRequestRule
{
    public List<string> Check(HolidayRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday request.</summary>
public sealed record HolidayRequestValidator(List<IHolidayRequestRule> Rules)
{
    public List<string> Validate(HolidayRequest item)
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

    public void RequireValid(HolidayRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayRequest> items)
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

/// <summary>Functions of holiday request rules.</summary>
public static class HolidayRequestRules
{
    public static HolidayRequestValidator DefaultHolidayRequestValidator()
    {
        return new HolidayRequestValidator(
            new List<IHolidayRequestRule>
            {
                new HolidayRequestNameLengthRule(16),
                new HolidayRequestRequiredJobCodeRule("holiday request needs an entry"),
                new HolidayRequestRevisionRangeRule(120, 255),
                new HolidayRequestCodeLengthRule(32),
                new HolidayRequestPriorityRangeRule(16, 86400),
                new HolidayRequestAllowedNameRule(new List<string> { "rolling" }),
            });
    }
}
