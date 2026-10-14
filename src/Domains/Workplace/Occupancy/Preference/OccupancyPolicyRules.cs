namespace Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>A rule a occupancy policy must pass. Check returns its problems, or none.</summary>
public interface IOccupancyPolicyRule
{
    List<string> Check(OccupancyPolicy item);
}

public sealed record OccupancyPolicyTitleLengthRule(int MaxLength) : IOccupancyPolicyRule
{
    public List<string> Check(OccupancyPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPolicyLabelLengthRule(int MaxLength) : IOccupancyPolicyRule
{
    public List<string> Check(OccupancyPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPolicyRequiredLabelRule(string Message) : IOccupancyPolicyRule
{
    public List<string> Check(OccupancyPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPolicyTimezoneLengthRule(int MaxLength) : IOccupancyPolicyRule
{
    public List<string> Check(OccupancyPolicy item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyPolicyBatchSizeRangeRule(int Min, int Max) : IOccupancyPolicyRule
{
    public List<string> Check(OccupancyPolicy item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a occupancy policy.</summary>
public sealed record OccupancyPolicyValidator(List<IOccupancyPolicyRule> Rules)
{
    public List<string> Validate(OccupancyPolicy item)
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

    public void RequireValid(OccupancyPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyPolicy> items)
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

/// <summary>Functions of occupancy policy rules.</summary>
public static class OccupancyPolicyRules
{
    public static OccupancyPolicyValidator DefaultOccupancyPolicyValidator()
    {
        return new OccupancyPolicyValidator(
            new List<IOccupancyPolicyRule>
            {
                new OccupancyPolicyTitleLengthRule(16),
                new OccupancyPolicyLabelLengthRule(16),
                new OccupancyPolicyRequiredLabelRule("occupancy policy needs this field"),
                new OccupancyPolicyTimezoneLengthRule(86400),
                new OccupancyPolicyBatchSizeRangeRule(3600, 3600),
            });
    }
}
