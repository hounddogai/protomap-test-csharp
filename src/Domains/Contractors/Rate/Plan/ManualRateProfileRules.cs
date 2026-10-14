namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual rate profile must pass. Check returns its problems, or none.</summary>
public interface IManualRateProfileRule
{
    List<string> Check(ManualRateProfile item);
}

public sealed record ManualRateProfileCodeLengthRule(int MaxLength) : IManualRateProfileRule
{
    public List<string> Check(ManualRateProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateProfileRequiredNameRule(string Message) : IManualRateProfileRule
{
    public List<string> Check(ManualRateProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualRateProfileLabelLengthRule(int MaxLength) : IManualRateProfileRule
{
    public List<string> Check(ManualRateProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateProfileAllowedLabelRule(List<string> Allowed) : IManualRateProfileRule
{
    public List<string> Check(ManualRateProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ManualRateProfileTenureMonthsRangeRule(int Min, int Max) : IManualRateProfileRule
{
    public List<string> Check(ManualRateProfile item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualRateProfileRequiredLabelRule(string Message) : IManualRateProfileRule
{
    public List<string> Check(ManualRateProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual rate profile.</summary>
public sealed record ManualRateProfileValidator(List<IManualRateProfileRule> Rules)
{
    public List<string> Validate(ManualRateProfile item)
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

    public void RequireValid(ManualRateProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRateProfile> items)
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

/// <summary>Functions of manual rate profile rules.</summary>
public static class ManualRateProfileRules
{
    public static ManualRateProfileValidator DefaultManualRateProfileValidator()
    {
        return new ManualRateProfileValidator(
            new List<IManualRateProfileRule>
            {
                new ManualRateProfileCodeLengthRule(32),
                new ManualRateProfileRequiredNameRule("manual rate profile needs a value"),
                new ManualRateProfileLabelLengthRule(32),
                new ManualRateProfileAllowedLabelRule(new List<string>()),
                new ManualRateProfileTenureMonthsRangeRule(32, 3600),
                new ManualRateProfileRequiredLabelRule("manual rate profile needs an entry"),
            });
    }
}
