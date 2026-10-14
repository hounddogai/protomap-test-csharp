namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient desk draft must pass. Check returns its problems, or none.</summary>
public interface ILenientDeskDraftRule
{
    List<string> Check(LenientDeskDraft item);
}

public sealed record LenientDeskDraftRequiredCodeRule(string Message) : ILenientDeskDraftRule
{
    public List<string> Check(LenientDeskDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskDraftAllowedDepartmentRule(List<string> Allowed) : ILenientDeskDraftRule
{
    public List<string> Check(LenientDeskDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskDraftCodeLengthRule(int MaxLength) : ILenientDeskDraftRule
{
    public List<string> Check(LenientDeskDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskDraftDepartmentLengthRule(int MaxLength) : ILenientDeskDraftRule
{
    public List<string> Check(LenientDeskDraft item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskDraftUpdatedAtRangeRule(int Min, int Max) : ILenientDeskDraftRule
{
    public List<string> Check(LenientDeskDraft item)
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

public sealed record LenientDeskDraftTitleLengthRule(int MaxLength) : ILenientDeskDraftRule
{
    public List<string> Check(LenientDeskDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient desk draft.</summary>
public sealed record LenientDeskDraftValidator(List<ILenientDeskDraftRule> Rules)
{
    public List<string> Validate(LenientDeskDraft item)
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

    public void RequireValid(LenientDeskDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeskDraft> items)
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

/// <summary>Functions of lenient desk draft rules.</summary>
public static class LenientDeskDraftRules
{
    public static LenientDeskDraftValidator DefaultLenientDeskDraftValidator()
    {
        return new LenientDeskDraftValidator(
            new List<ILenientDeskDraftRule>
            {
                new LenientDeskDraftRequiredCodeRule("lenient desk draft needs a value"),
                new LenientDeskDraftAllowedDepartmentRule(new List<string>()),
                new LenientDeskDraftCodeLengthRule(3600),
                new LenientDeskDraftDepartmentLengthRule(86400),
                new LenientDeskDraftUpdatedAtRangeRule(16, 255),
                new LenientDeskDraftTitleLengthRule(32),
            });
    }
}
