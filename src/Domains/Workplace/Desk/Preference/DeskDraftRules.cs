namespace Acme.Workforce.Workplace.Desk.Preference;

/// <summary>A rule a desk draft must pass. Check returns its problems, or none.</summary>
public interface IDeskDraftRule
{
    List<string> Check(DeskDraft item);
}

public sealed record DeskDraftVersionRangeRule(int Min, int Max) : IDeskDraftRule
{
    public List<string> Check(DeskDraft item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskDraftAttemptsRangeRule(int Min, int Max) : IDeskDraftRule
{
    public List<string> Check(DeskDraft item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskDraftRequiredGradeRule(string Message) : IDeskDraftRule
{
    public List<string> Check(DeskDraft item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskDraftTitleLengthRule(int MaxLength) : IDeskDraftRule
{
    public List<string> Check(DeskDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk draft.</summary>
public sealed record DeskDraftValidator(List<IDeskDraftRule> Rules)
{
    public List<string> Validate(DeskDraft item)
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

    public void RequireValid(DeskDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskDraft> items)
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

/// <summary>Functions of desk draft rules.</summary>
public static class DeskDraftRules
{
    public static DeskDraftValidator DefaultDeskDraftValidator()
    {
        return new DeskDraftValidator(
            new List<IDeskDraftRule>
            {
                new DeskDraftVersionRangeRule(16, 120),
                new DeskDraftAttemptsRangeRule(120, 1000),
                new DeskDraftRequiredGradeRule("desk draft needs this field"),
                new DeskDraftTitleLengthRule(86400),
            });
    }
}
