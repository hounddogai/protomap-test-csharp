namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped audit plan must pass. Check returns its problems, or none.</summary>
public interface ICappedAuditPlanRule
{
    List<string> Check(CappedAuditPlan item);
}

public sealed record CappedAuditPlanDurationMinutesRangeRule(int Min, int Max) : ICappedAuditPlanRule
{
    public List<string> Check(CappedAuditPlan item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditPlanLabelLengthRule(int MaxLength) : ICappedAuditPlanRule
{
    public List<string> Check(CappedAuditPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditPlanAllowedLabelRule(List<string> Allowed) : ICappedAuditPlanRule
{
    public List<string> Check(CappedAuditPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditPlanRatioRangeRule(double Min, double Max) : ICappedAuditPlanRule
{
    public List<string> Check(CappedAuditPlan item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAuditPlanRequiredLabelRule(string Message) : ICappedAuditPlanRule
{
    public List<string> Check(CappedAuditPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped audit plan.</summary>
public sealed record CappedAuditPlanValidator(List<ICappedAuditPlanRule> Rules)
{
    public List<string> Validate(CappedAuditPlan item)
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

    public void RequireValid(CappedAuditPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAuditPlan> items)
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

/// <summary>Functions of capped audit plan rules.</summary>
public static class CappedAuditPlanRules
{
    public static CappedAuditPlanValidator DefaultCappedAuditPlanValidator()
    {
        return new CappedAuditPlanValidator(
            new List<ICappedAuditPlanRule>
            {
                new CappedAuditPlanDurationMinutesRangeRule(16, 255),
                new CappedAuditPlanLabelLengthRule(1000),
                new CappedAuditPlanAllowedLabelRule(new List<string> { "default", "rolling", "primary" }),
                new CappedAuditPlanRatioRangeRule(2.4, 3896.8),
                new CappedAuditPlanRequiredLabelRule("capped audit plan needs an entry"),
            });
    }
}
