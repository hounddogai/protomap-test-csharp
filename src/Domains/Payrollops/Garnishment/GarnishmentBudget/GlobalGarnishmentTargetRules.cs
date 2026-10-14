namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>A rule a global garnishment target must pass. Check returns its problems, or none.</summary>
public interface IGlobalGarnishmentTargetRule
{
    List<string> Check(GlobalGarnishmentTarget item);
}

public sealed record GlobalGarnishmentTargetGradeLengthRule(int MaxLength) : IGlobalGarnishmentTargetRule
{
    public List<string> Check(GlobalGarnishmentTarget item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalGarnishmentTargetHoursRangeRule(double Min, double Max) : IGlobalGarnishmentTargetRule
{
    public List<string> Check(GlobalGarnishmentTarget item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalGarnishmentTargetRequiredGradeRule(string Message) : IGlobalGarnishmentTargetRule
{
    public List<string> Check(GlobalGarnishmentTarget item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalGarnishmentTargetRequiredTitleRule(string Message) : IGlobalGarnishmentTargetRule
{
    public List<string> Check(GlobalGarnishmentTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalGarnishmentTargetRequiredCodeRule(string Message) : IGlobalGarnishmentTargetRule
{
    public List<string> Check(GlobalGarnishmentTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global garnishment target.</summary>
public sealed record GlobalGarnishmentTargetValidator(List<IGlobalGarnishmentTargetRule> Rules)
{
    public List<string> Validate(GlobalGarnishmentTarget item)
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

    public void RequireValid(GlobalGarnishmentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalGarnishmentTarget> items)
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

/// <summary>Functions of global garnishment target rules.</summary>
public static class GlobalGarnishmentTargetRules
{
    public static GlobalGarnishmentTargetValidator DefaultGlobalGarnishmentTargetValidator()
    {
        return new GlobalGarnishmentTargetValidator(
            new List<IGlobalGarnishmentTargetRule>
            {
                new GlobalGarnishmentTargetGradeLengthRule(3600),
                new GlobalGarnishmentTargetHoursRangeRule(3.6, 3646.4),
                new GlobalGarnishmentTargetRequiredGradeRule("global garnishment target needs a value"),
                new GlobalGarnishmentTargetRequiredTitleRule("global garnishment target needs a value"),
                new GlobalGarnishmentTargetRequiredCodeRule("global garnishment target needs an entry"),
            });
    }
}
