namespace Acme.Workforce.Workplace.Desk.Preference;

/// <summary>A rule a weighted desk preference must pass. Check returns its problems, or none.</summary>
public interface IWeightedDeskPreferenceRule
{
    List<string> Check(WeightedDeskPreference item);
}

public sealed record WeightedDeskPreferenceRequiredCodeRule(string Message) : IWeightedDeskPreferenceRule
{
    public List<string> Check(WeightedDeskPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeskPreferenceRequiredSalaryBandRule(string Message) : IWeightedDeskPreferenceRule
{
    public List<string> Check(WeightedDeskPreference item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeskPreferenceCodeLengthRule(int MaxLength) : IWeightedDeskPreferenceRule
{
    public List<string> Check(WeightedDeskPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeskPreferenceRequiredLabelRule(string Message) : IWeightedDeskPreferenceRule
{
    public List<string> Check(WeightedDeskPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted desk preference.</summary>
public sealed record WeightedDeskPreferenceValidator(List<IWeightedDeskPreferenceRule> Rules)
{
    public List<string> Validate(WeightedDeskPreference item)
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

    public void RequireValid(WeightedDeskPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedDeskPreference> items)
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

/// <summary>Functions of weighted desk preference rules.</summary>
public static class WeightedDeskPreferenceRules
{
    public static WeightedDeskPreferenceValidator DefaultWeightedDeskPreferenceValidator()
    {
        return new WeightedDeskPreferenceValidator(
            new List<IWeightedDeskPreferenceRule>
            {
                new WeightedDeskPreferenceRequiredCodeRule("weighted desk preference needs this field"),
                new WeightedDeskPreferenceRequiredSalaryBandRule("weighted desk preference needs an entry"),
                new WeightedDeskPreferenceCodeLengthRule(64),
                new WeightedDeskPreferenceRequiredLabelRule("weighted desk preference needs this field"),
            });
    }
}
