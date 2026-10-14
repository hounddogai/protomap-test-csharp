namespace Acme.Workforce.Absence.Balance.Summary;

/// <summary>A rule a default balance draft must pass. Check returns its problems, or none.</summary>
public interface IDefaultBalanceDraftRule
{
    List<string> Check(DefaultBalanceDraft item);
}

public sealed record DefaultBalanceDraftSummaryLengthRule(int MaxLength) : IDefaultBalanceDraftRule
{
    public List<string> Check(DefaultBalanceDraft item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBalanceDraftRequiredSummaryRule(string Message) : IDefaultBalanceDraftRule
{
    public List<string> Check(DefaultBalanceDraft item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBalanceDraftLeaveDaysRangeRule(int Min, int Max) : IDefaultBalanceDraftRule
{
    public List<string> Check(DefaultBalanceDraft item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultBalanceDraftCodeLengthRule(int MaxLength) : IDefaultBalanceDraftRule
{
    public List<string> Check(DefaultBalanceDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBalanceDraftTitleLengthRule(int MaxLength) : IDefaultBalanceDraftRule
{
    public List<string> Check(DefaultBalanceDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default balance draft.</summary>
public sealed record DefaultBalanceDraftValidator(List<IDefaultBalanceDraftRule> Rules)
{
    public List<string> Validate(DefaultBalanceDraft item)
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

    public void RequireValid(DefaultBalanceDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBalanceDraft> items)
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

/// <summary>Functions of default balance draft rules.</summary>
public static class DefaultBalanceDraftRules
{
    public static DefaultBalanceDraftValidator DefaultDefaultBalanceDraftValidator()
    {
        return new DefaultBalanceDraftValidator(
            new List<IDefaultBalanceDraftRule>
            {
                new DefaultBalanceDraftSummaryLengthRule(32),
                new DefaultBalanceDraftRequiredSummaryRule("default balance draft needs this field"),
                new DefaultBalanceDraftLeaveDaysRangeRule(80, 86400),
                new DefaultBalanceDraftCodeLengthRule(86400),
                new DefaultBalanceDraftTitleLengthRule(16),
            });
    }
}
