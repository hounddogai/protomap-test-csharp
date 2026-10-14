namespace Acme.Workforce.Assets.Assignment.Profile;

/// <summary>A rule a capped assignment preference must pass. Check returns its problems, or none.</summary>
public interface ICappedAssignmentPreferenceRule
{
    List<string> Check(CappedAssignmentPreference item);
}

public sealed record CappedAssignmentPreferenceCodePrefixRule(string Prefix) : ICappedAssignmentPreferenceRule
{
    public List<string> Check(CappedAssignmentPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssignmentPreferenceBudgetRangeRule(double Min, double Max) : ICappedAssignmentPreferenceRule
{
    public List<string> Check(CappedAssignmentPreference item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAssignmentPreferenceGraceDaysRangeRule(int Min, int Max) : ICappedAssignmentPreferenceRule
{
    public List<string> Check(CappedAssignmentPreference item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAssignmentPreferenceCodeLengthRule(int MaxLength) : ICappedAssignmentPreferenceRule
{
    public List<string> Check(CappedAssignmentPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped assignment preference.</summary>
public sealed record CappedAssignmentPreferenceValidator(List<ICappedAssignmentPreferenceRule> Rules)
{
    public List<string> Validate(CappedAssignmentPreference item)
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

    public void RequireValid(CappedAssignmentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAssignmentPreference> items)
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

/// <summary>Functions of capped assignment preference rules.</summary>
public static class CappedAssignmentPreferenceRules
{
    public static CappedAssignmentPreferenceValidator DefaultCappedAssignmentPreferenceValidator()
    {
        return new CappedAssignmentPreferenceValidator(
            new List<ICappedAssignmentPreferenceRule>
            {
                new CappedAssignmentPreferenceCodePrefixRule("Q"),
                new CappedAssignmentPreferenceBudgetRangeRule(5.9, 3649.9),
                new CappedAssignmentPreferenceGraceDaysRangeRule(32, 120),
                new CappedAssignmentPreferenceCodeLengthRule(3600),
            });
    }
}
