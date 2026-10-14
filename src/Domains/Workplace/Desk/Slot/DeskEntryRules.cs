namespace Acme.Workforce.Workplace.Desk.Slot;

/// <summary>A rule a desk entry must pass. Check returns its problems, or none.</summary>
public interface IDeskEntryRule
{
    List<string> Check(DeskEntry item);
}

public sealed record DeskEntryRequiredEmployeeIdRule(string Message) : IDeskEntryRule
{
    public List<string> Check(DeskEntry item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskEntryCodeLengthRule(int MaxLength) : IDeskEntryRule
{
    public List<string> Check(DeskEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskEntryDepartmentLengthRule(int MaxLength) : IDeskEntryRule
{
    public List<string> Check(DeskEntry item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk entry.</summary>
public sealed record DeskEntryValidator(List<IDeskEntryRule> Rules)
{
    public List<string> Validate(DeskEntry item)
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

    public void RequireValid(DeskEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskEntry> items)
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

/// <summary>Functions of desk entry rules.</summary>
public static class DeskEntryRules
{
    public static DeskEntryValidator DefaultDeskEntryValidator()
    {
        return new DeskEntryValidator(
            new List<IDeskEntryRule>
            {
                new DeskEntryRequiredEmployeeIdRule("desk entry needs a value"),
                new DeskEntryCodeLengthRule(80),
                new DeskEntryDepartmentLengthRule(64),
            });
    }
}
