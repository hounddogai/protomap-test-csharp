namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>A rule a headcount allocation must pass. Check returns its problems, or none.</summary>
public interface IHeadcountAllocationRule
{
    List<string> Check(HeadcountAllocation item);
}

public sealed record HeadcountAllocationLabelLengthRule(int MaxLength) : IHeadcountAllocationRule
{
    public List<string> Check(HeadcountAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAllocationNotesLengthRule(int MaxLength) : IHeadcountAllocationRule
{
    public List<string> Check(HeadcountAllocation item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAllocationRequiredLabelRule(string Message) : IHeadcountAllocationRule
{
    public List<string> Check(HeadcountAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount allocation.</summary>
public sealed record HeadcountAllocationValidator(List<IHeadcountAllocationRule> Rules)
{
    public List<string> Validate(HeadcountAllocation item)
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

    public void RequireValid(HeadcountAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountAllocation> items)
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

/// <summary>Functions of headcount allocation rules.</summary>
public static class HeadcountAllocationRules
{
    public static HeadcountAllocationValidator DefaultHeadcountAllocationValidator()
    {
        return new HeadcountAllocationValidator(
            new List<IHeadcountAllocationRule>
            {
                new HeadcountAllocationLabelLengthRule(255),
                new HeadcountAllocationNotesLengthRule(64),
                new HeadcountAllocationRequiredLabelRule("headcount allocation needs an entry"),
            });
    }
}
