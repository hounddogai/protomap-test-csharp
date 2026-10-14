namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global nomination milestone must pass. Check returns its problems, or none.</summary>
public interface IGlobalNominationMilestoneRule
{
    List<string> Check(GlobalNominationMilestone item);
}

public sealed record GlobalNominationMilestoneAllowedLabelRule(List<string> Allowed) : IGlobalNominationMilestoneRule
{
    public List<string> Check(GlobalNominationMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationMilestoneDiscountRangeRule(double Min, double Max) : IGlobalNominationMilestoneRule
{
    public List<string> Check(GlobalNominationMilestone item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationMilestoneCodeLengthRule(int MaxLength) : IGlobalNominationMilestoneRule
{
    public List<string> Check(GlobalNominationMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationMilestoneMaximumRangeRule(double Min, double Max) : IGlobalNominationMilestoneRule
{
    public List<string> Check(GlobalNominationMilestone item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalNominationMilestoneRatioRangeRule(double Min, double Max) : IGlobalNominationMilestoneRule
{
    public List<string> Check(GlobalNominationMilestone item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalNominationMilestoneExpiresAtRangeRule(int Min, int Max) : IGlobalNominationMilestoneRule
{
    public List<string> Check(GlobalNominationMilestone item)
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

/// <summary>Runs every rule of a global nomination milestone.</summary>
public sealed record GlobalNominationMilestoneValidator(List<IGlobalNominationMilestoneRule> Rules)
{
    public List<string> Validate(GlobalNominationMilestone item)
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

    public void RequireValid(GlobalNominationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalNominationMilestone> items)
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

/// <summary>Functions of global nomination milestone rules.</summary>
public static class GlobalNominationMilestoneRules
{
    public static GlobalNominationMilestoneValidator DefaultGlobalNominationMilestoneValidator()
    {
        return new GlobalNominationMilestoneValidator(
            new List<IGlobalNominationMilestoneRule>
            {
                new GlobalNominationMilestoneAllowedLabelRule(new List<string>()),
                new GlobalNominationMilestoneDiscountRangeRule(9.2, 2913.6),
                new GlobalNominationMilestoneCodeLengthRule(120),
                new GlobalNominationMilestoneMaximumRangeRule(4.7, 831.1),
                new GlobalNominationMilestoneRatioRangeRule(6.1, 249.9),
                new GlobalNominationMilestoneExpiresAtRangeRule(16, 64),
            });
    }
}
