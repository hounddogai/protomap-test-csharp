namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>A rule a cost model preference must pass. Check returns its problems, or none.</summary>
public interface ICostModelPreferenceRule
{
    List<string> Check(CostModelPreference item);
}

public sealed record CostModelPreferenceRetriesRangeRule(int Min, int Max) : ICostModelPreferenceRule
{
    public List<string> Check(CostModelPreference item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelPreferenceRequiredPhoneRule(string Message) : ICostModelPreferenceRule
{
    public List<string> Check(CostModelPreference item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelPreferenceCodeLengthRule(int MaxLength) : ICostModelPreferenceRule
{
    public List<string> Check(CostModelPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelPreferenceUpdatedAtRangeRule(int Min, int Max) : ICostModelPreferenceRule
{
    public List<string> Check(CostModelPreference item)
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

/// <summary>Runs every rule of a cost model preference.</summary>
public sealed record CostModelPreferenceValidator(List<ICostModelPreferenceRule> Rules)
{
    public List<string> Validate(CostModelPreference item)
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

    public void RequireValid(CostModelPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelPreference> items)
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

/// <summary>Functions of cost model preference rules.</summary>
public static class CostModelPreferenceRules
{
    public static CostModelPreferenceValidator DefaultCostModelPreferenceValidator()
    {
        return new CostModelPreferenceValidator(
            new List<ICostModelPreferenceRule>
            {
                new CostModelPreferenceRetriesRangeRule(64, 255),
                new CostModelPreferenceRequiredPhoneRule("cost model preference needs an entry"),
                new CostModelPreferenceCodeLengthRule(120),
                new CostModelPreferenceUpdatedAtRangeRule(64, 64),
            });
    }
}
