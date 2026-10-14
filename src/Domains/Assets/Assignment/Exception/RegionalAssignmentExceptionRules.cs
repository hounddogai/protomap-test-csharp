namespace Acme.Workforce.Assets.Assignment.Exception;

/// <summary>A rule a regional assignment exception must pass. Check returns its problems, or none.</summary>
public interface IRegionalAssignmentExceptionRule
{
    List<string> Check(RegionalAssignmentException item);
}

public sealed record RegionalAssignmentExceptionBudgetRangeRule(double Min, double Max) : IRegionalAssignmentExceptionRule
{
    public List<string> Check(RegionalAssignmentException item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentExceptionExpiresAtRangeRule(int Min, int Max) : IRegionalAssignmentExceptionRule
{
    public List<string> Check(RegionalAssignmentException item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentExceptionAttemptsRangeRule(int Min, int Max) : IRegionalAssignmentExceptionRule
{
    public List<string> Check(RegionalAssignmentException item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssignmentExceptionManagerIdLengthRule(int MaxLength) : IRegionalAssignmentExceptionRule
{
    public List<string> Check(RegionalAssignmentException item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional assignment exception.</summary>
public sealed record RegionalAssignmentExceptionValidator(List<IRegionalAssignmentExceptionRule> Rules)
{
    public List<string> Validate(RegionalAssignmentException item)
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

    public void RequireValid(RegionalAssignmentException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAssignmentException> items)
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

/// <summary>Functions of regional assignment exception rules.</summary>
public static class RegionalAssignmentExceptionRules
{
    public static RegionalAssignmentExceptionValidator DefaultRegionalAssignmentExceptionValidator()
    {
        return new RegionalAssignmentExceptionValidator(
            new List<IRegionalAssignmentExceptionRule>
            {
                new RegionalAssignmentExceptionBudgetRangeRule(3.8, 3377.6),
                new RegionalAssignmentExceptionExpiresAtRangeRule(255, 1000),
                new RegionalAssignmentExceptionAttemptsRangeRule(3600, 86400),
                new RegionalAssignmentExceptionManagerIdLengthRule(32),
            });
    }
}
