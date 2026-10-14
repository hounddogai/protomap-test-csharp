namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>A rule a primary garnishment mapping must pass. Check returns its problems, or none.</summary>
public interface IPrimaryGarnishmentMappingRule
{
    List<string> Check(PrimaryGarnishmentMapping item);
}

public sealed record PrimaryGarnishmentMappingRequiredNameRule(string Message) : IPrimaryGarnishmentMappingRule
{
    public List<string> Check(PrimaryGarnishmentMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentMappingUpdatedAtRangeRule(int Min, int Max) : IPrimaryGarnishmentMappingRule
{
    public List<string> Check(PrimaryGarnishmentMapping item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryGarnishmentMappingCodeLengthRule(int MaxLength) : IPrimaryGarnishmentMappingRule
{
    public List<string> Check(PrimaryGarnishmentMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentMappingCeilingRangeRule(double Min, double Max) : IPrimaryGarnishmentMappingRule
{
    public List<string> Check(PrimaryGarnishmentMapping item)
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

public sealed record PrimaryGarnishmentMappingRequiredEmployeeIdRule(string Message) : IPrimaryGarnishmentMappingRule
{
    public List<string> Check(PrimaryGarnishmentMapping item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryGarnishmentMappingExternalRefLengthRule(int MaxLength) : IPrimaryGarnishmentMappingRule
{
    public List<string> Check(PrimaryGarnishmentMapping item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary garnishment mapping.</summary>
public sealed record PrimaryGarnishmentMappingValidator(List<IPrimaryGarnishmentMappingRule> Rules)
{
    public List<string> Validate(PrimaryGarnishmentMapping item)
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

    public void RequireValid(PrimaryGarnishmentMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryGarnishmentMapping> items)
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

/// <summary>Functions of primary garnishment mapping rules.</summary>
public static class PrimaryGarnishmentMappingRules
{
    public static PrimaryGarnishmentMappingValidator DefaultPrimaryGarnishmentMappingValidator()
    {
        return new PrimaryGarnishmentMappingValidator(
            new List<IPrimaryGarnishmentMappingRule>
            {
                new PrimaryGarnishmentMappingRequiredNameRule("primary garnishment mapping needs this field"),
                new PrimaryGarnishmentMappingUpdatedAtRangeRule(255, 1000),
                new PrimaryGarnishmentMappingCodeLengthRule(120),
                new PrimaryGarnishmentMappingCeilingRangeRule(4.9, 413.6),
                new PrimaryGarnishmentMappingRequiredEmployeeIdRule("primary garnishment mapping needs this field"),
                new PrimaryGarnishmentMappingExternalRefLengthRule(255),
            });
    }
}
