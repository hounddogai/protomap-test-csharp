namespace Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>A rule a accrual preference must pass. Check returns its problems, or none.</summary>
public interface IAccrualPreferenceRule
{
    List<string> Check(AccrualPreference item);
}

public sealed record AccrualPreferenceCodePrefixRule(string Prefix) : IAccrualPreferenceRule
{
    public List<string> Check(AccrualPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPreferenceFloorValueRangeRule(double Min, double Max) : IAccrualPreferenceRule
{
    public List<string> Check(AccrualPreference item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPreferenceNameLengthRule(int MaxLength) : IAccrualPreferenceRule
{
    public List<string> Check(AccrualPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualPreferenceRatingRangeRule(double Min, double Max) : IAccrualPreferenceRule
{
    public List<string> Check(AccrualPreference item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a accrual preference.</summary>
public sealed record AccrualPreferenceValidator(List<IAccrualPreferenceRule> Rules)
{
    public List<string> Validate(AccrualPreference item)
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

    public void RequireValid(AccrualPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualPreference> items)
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

/// <summary>Functions of accrual preference rules.</summary>
public static class AccrualPreferenceRules
{
    public static AccrualPreferenceValidator DefaultAccrualPreferenceValidator()
    {
        return new AccrualPreferenceValidator(
            new List<IAccrualPreferenceRule>
            {
                new AccrualPreferenceCodePrefixRule("ACM"),
                new AccrualPreferenceFloorValueRangeRule(7.5, 2684.5),
                new AccrualPreferenceNameLengthRule(64),
                new AccrualPreferenceRatingRangeRule(8.3, 84.9),
            });
    }
}
