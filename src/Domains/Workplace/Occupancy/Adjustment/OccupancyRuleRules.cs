namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a occupancy rule must pass. Check returns its problems, or none.</summary>
public interface IOccupancyRuleRule
{
    List<string> Check(OccupancyRule item);
}

public sealed record OccupancyRuleAllowedCodeRule(List<string> Allowed) : IOccupancyRuleRule
{
    public List<string> Check(OccupancyRule item)
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

public sealed record OccupancyRulePhoneLengthRule(int MaxLength) : IOccupancyRuleRule
{
    public List<string> Check(OccupancyRule item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRuleTitleLengthRule(int MaxLength) : IOccupancyRuleRule
{
    public List<string> Check(OccupancyRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy rule.</summary>
public sealed record OccupancyRuleValidator(List<IOccupancyRuleRule> Rules)
{
    public List<string> Validate(OccupancyRule item)
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

    public void RequireValid(OccupancyRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyRule> items)
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

/// <summary>Functions of occupancy rule rules.</summary>
public static class OccupancyRuleRules
{
    public static OccupancyRuleValidator DefaultOccupancyRuleValidator()
    {
        return new OccupancyRuleValidator(
            new List<IOccupancyRuleRule>
            {
                new OccupancyRuleAllowedCodeRule(new List<string> { "tiered", "default" }),
                new OccupancyRulePhoneLengthRule(64),
                new OccupancyRuleTitleLengthRule(3600),
            });
    }
}
