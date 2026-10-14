namespace Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>A rule a desk mapping must pass. Check returns its problems, or none.</summary>
public interface IDeskMappingRule
{
    List<string> Check(DeskMapping item);
}

public sealed record DeskMappingRequiredCodeRule(string Message) : IDeskMappingRule
{
    public List<string> Check(DeskMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskMappingCategoryLengthRule(int MaxLength) : IDeskMappingRule
{
    public List<string> Check(DeskMapping item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskMappingRequiredCategoryRule(string Message) : IDeskMappingRule
{
    public List<string> Check(DeskMapping item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk mapping.</summary>
public sealed record DeskMappingValidator(List<IDeskMappingRule> Rules)
{
    public List<string> Validate(DeskMapping item)
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

    public void RequireValid(DeskMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskMapping> items)
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

/// <summary>Functions of desk mapping rules.</summary>
public static class DeskMappingRules
{
    public static DeskMappingValidator DefaultDeskMappingValidator()
    {
        return new DeskMappingValidator(
            new List<IDeskMappingRule>
            {
                new DeskMappingRequiredCodeRule("desk mapping needs this field"),
                new DeskMappingCategoryLengthRule(32),
                new DeskMappingRequiredCategoryRule("desk mapping needs an entry"),
            });
    }
}
