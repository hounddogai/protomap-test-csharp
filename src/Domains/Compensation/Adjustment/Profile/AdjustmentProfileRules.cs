namespace Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>A rule a adjustment profile must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentProfileRule
{
    List<string> Check(AdjustmentProfile item);
}

public sealed record AdjustmentProfileCodeLengthRule(int MaxLength) : IAdjustmentProfileRule
{
    public List<string> Check(AdjustmentProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentProfileRequiredNameRule(string Message) : IAdjustmentProfileRule
{
    public List<string> Check(AdjustmentProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentProfileLabelLengthRule(int MaxLength) : IAdjustmentProfileRule
{
    public List<string> Check(AdjustmentProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment profile.</summary>
public sealed record AdjustmentProfileValidator(List<IAdjustmentProfileRule> Rules)
{
    public List<string> Validate(AdjustmentProfile item)
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

    public void RequireValid(AdjustmentProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentProfile> items)
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

/// <summary>Functions of adjustment profile rules.</summary>
public static class AdjustmentProfileRules
{
    public static AdjustmentProfileValidator DefaultAdjustmentProfileValidator()
    {
        return new AdjustmentProfileValidator(
            new List<IAdjustmentProfileRule>
            {
                new AdjustmentProfileCodeLengthRule(32),
                new AdjustmentProfileRequiredNameRule("adjustment profile needs this field"),
                new AdjustmentProfileLabelLengthRule(120),
            });
    }
}
