namespace Acme.Workforce.Workplace.Desk.Agreement;

/// <summary>A rule a lenient desk forecast must pass. Check returns its problems, or none.</summary>
public interface ILenientDeskForecastRule
{
    List<string> Check(LenientDeskForecast item);
}

public sealed record LenientDeskForecastDepartmentLengthRule(int MaxLength) : ILenientDeskForecastRule
{
    public List<string> Check(LenientDeskForecast item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskForecastCodeLengthRule(int MaxLength) : ILenientDeskForecastRule
{
    public List<string> Check(LenientDeskForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskForecastMaximumRangeRule(double Min, double Max) : ILenientDeskForecastRule
{
    public List<string> Check(LenientDeskForecast item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskForecastRequiredDepartmentRule(string Message) : ILenientDeskForecastRule
{
    public List<string> Check(LenientDeskForecast item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient desk forecast.</summary>
public sealed record LenientDeskForecastValidator(List<ILenientDeskForecastRule> Rules)
{
    public List<string> Validate(LenientDeskForecast item)
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

    public void RequireValid(LenientDeskForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeskForecast> items)
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

/// <summary>Functions of lenient desk forecast rules.</summary>
public static class LenientDeskForecastRules
{
    public static LenientDeskForecastValidator DefaultLenientDeskForecastValidator()
    {
        return new LenientDeskForecastValidator(
            new List<ILenientDeskForecastRule>
            {
                new LenientDeskForecastDepartmentLengthRule(64),
                new LenientDeskForecastCodeLengthRule(255),
                new LenientDeskForecastMaximumRangeRule(7.1, 2881.5),
                new LenientDeskForecastRequiredDepartmentRule("lenient desk forecast needs this field"),
            });
    }
}
