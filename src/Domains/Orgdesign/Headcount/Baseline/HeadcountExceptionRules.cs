namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>A rule a headcount exception must pass. Check returns its problems, or none.</summary>
public interface IHeadcountExceptionRule
{
    List<string> Check(HeadcountException item);
}

public sealed record HeadcountExceptionNameLengthRule(int MaxLength) : IHeadcountExceptionRule
{
    public List<string> Check(HeadcountException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountExceptionRequiredCodeRule(string Message) : IHeadcountExceptionRule
{
    public List<string> Check(HeadcountException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountExceptionCeilingRangeRule(double Min, double Max) : IHeadcountExceptionRule
{
    public List<string> Check(HeadcountException item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountExceptionVersionRangeRule(int Min, int Max) : IHeadcountExceptionRule
{
    public List<string> Check(HeadcountException item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HeadcountExceptionTeamLengthRule(int MaxLength) : IHeadcountExceptionRule
{
    public List<string> Check(HeadcountException item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount exception.</summary>
public sealed record HeadcountExceptionValidator(List<IHeadcountExceptionRule> Rules)
{
    public List<string> Validate(HeadcountException item)
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

    public void RequireValid(HeadcountException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountException> items)
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

/// <summary>Functions of headcount exception rules.</summary>
public static class HeadcountExceptionRules
{
    public static HeadcountExceptionValidator DefaultHeadcountExceptionValidator()
    {
        return new HeadcountExceptionValidator(
            new List<IHeadcountExceptionRule>
            {
                new HeadcountExceptionNameLengthRule(3600),
                new HeadcountExceptionRequiredCodeRule("headcount exception needs this field"),
                new HeadcountExceptionCeilingRangeRule(8.4, 4403.1),
                new HeadcountExceptionVersionRangeRule(32, 255),
                new HeadcountExceptionTeamLengthRule(86400),
            });
    }
}
