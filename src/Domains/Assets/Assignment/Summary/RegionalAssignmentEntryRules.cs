namespace Acme.Workforce.Assets.Assignment.Summary;

/// <summary>A rule a regional assignment entry must pass. Check returns its problems, or none.</summary>
public interface IRegionalAssignmentEntryRule
{
    List<string> Check(RegionalAssignmentEntry item);
}

public sealed record RegionalAssignmentEntryTitleLengthRule(int MaxLength) : IRegionalAssignmentEntryRule
{
    public List<string> Check(RegionalAssignmentEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentEntryCodeLengthRule(int MaxLength) : IRegionalAssignmentEntryRule
{
    public List<string> Check(RegionalAssignmentEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentEntryRequiredCodeRule(string Message) : IRegionalAssignmentEntryRule
{
    public List<string> Check(RegionalAssignmentEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional assignment entry.</summary>
public sealed record RegionalAssignmentEntryValidator(List<IRegionalAssignmentEntryRule> Rules)
{
    public List<string> Validate(RegionalAssignmentEntry item)
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

    public void RequireValid(RegionalAssignmentEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAssignmentEntry> items)
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

/// <summary>Functions of regional assignment entry rules.</summary>
public static class RegionalAssignmentEntryRules
{
    public static RegionalAssignmentEntryValidator DefaultRegionalAssignmentEntryValidator()
    {
        return new RegionalAssignmentEntryValidator(
            new List<IRegionalAssignmentEntryRule>
            {
                new RegionalAssignmentEntryTitleLengthRule(3600),
                new RegionalAssignmentEntryCodeLengthRule(86400),
                new RegionalAssignmentEntryRequiredCodeRule("regional assignment entry needs a value"),
            });
    }
}
