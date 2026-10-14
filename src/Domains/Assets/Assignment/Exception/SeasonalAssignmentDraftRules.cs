namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal assignment draft must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAssignmentDraftRule
{
    List<string> Check(SeasonalAssignmentDraft item);
}

public sealed record SeasonalAssignmentDraftAllowedCodeRule(List<string> Allowed) : ISeasonalAssignmentDraftRule
{
    public List<string> Check(SeasonalAssignmentDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentDraftRequiredNameRule(string Message) : ISeasonalAssignmentDraftRule
{
    public List<string> Check(SeasonalAssignmentDraft item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentDraftSalaryBandLengthRule(int MaxLength) : ISeasonalAssignmentDraftRule
{
    public List<string> Check(SeasonalAssignmentDraft item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentDraftRequiredSalaryBandRule(string Message) : ISeasonalAssignmentDraftRule
{
    public List<string> Check(SeasonalAssignmentDraft item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAssignmentDraftExpiresAtRangeRule(int Min, int Max) : ISeasonalAssignmentDraftRule
{
    public List<string> Check(SeasonalAssignmentDraft item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalAssignmentDraftEmployeeIdLengthRule(int MaxLength) : ISeasonalAssignmentDraftRule
{
    public List<string> Check(SeasonalAssignmentDraft item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal assignment draft.</summary>
public sealed record SeasonalAssignmentDraftValidator(List<ISeasonalAssignmentDraftRule> Rules)
{
    public List<string> Validate(SeasonalAssignmentDraft item)
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

    public void RequireValid(SeasonalAssignmentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAssignmentDraft> items)
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

/// <summary>Functions of seasonal assignment draft rules.</summary>
public static class SeasonalAssignmentDraftRules
{
    public static SeasonalAssignmentDraftValidator DefaultSeasonalAssignmentDraftValidator()
    {
        return new SeasonalAssignmentDraftValidator(
            new List<ISeasonalAssignmentDraftRule>
            {
                new SeasonalAssignmentDraftAllowedCodeRule(new List<string> { "global", "fallback", "scheduled" }),
                new SeasonalAssignmentDraftRequiredNameRule("seasonal assignment draft needs an entry"),
                new SeasonalAssignmentDraftSalaryBandLengthRule(80),
                new SeasonalAssignmentDraftRequiredSalaryBandRule("seasonal assignment draft needs a value"),
                new SeasonalAssignmentDraftExpiresAtRangeRule(32, 86400),
                new SeasonalAssignmentDraftEmployeeIdLengthRule(86400),
            });
    }
}
