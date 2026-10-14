namespace Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>A rule a rolling succession plan quota must pass. Check returns its problems, or none.</summary>
public interface IRollingSuccessionPlanQuotaRule
{
    List<string> Check(RollingSuccessionPlanQuota item);
}

public sealed record RollingSuccessionPlanQuotaCodeLengthRule(int MaxLength) : IRollingSuccessionPlanQuotaRule
{
    public List<string> Check(RollingSuccessionPlanQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanQuotaRequiredSourceRule(string Message) : IRollingSuccessionPlanQuotaRule
{
    public List<string> Check(RollingSuccessionPlanQuota item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanQuotaLabelLengthRule(int MaxLength) : IRollingSuccessionPlanQuotaRule
{
    public List<string> Check(RollingSuccessionPlanQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling succession plan quota.</summary>
public sealed record RollingSuccessionPlanQuotaValidator(List<IRollingSuccessionPlanQuotaRule> Rules)
{
    public List<string> Validate(RollingSuccessionPlanQuota item)
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

    public void RequireValid(RollingSuccessionPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSuccessionPlanQuota> items)
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

/// <summary>Functions of rolling succession plan quota rules.</summary>
public static class RollingSuccessionPlanQuotaRules
{
    public static RollingSuccessionPlanQuotaValidator DefaultRollingSuccessionPlanQuotaValidator()
    {
        return new RollingSuccessionPlanQuotaValidator(
            new List<IRollingSuccessionPlanQuotaRule>
            {
                new RollingSuccessionPlanQuotaCodeLengthRule(255),
                new RollingSuccessionPlanQuotaRequiredSourceRule("rolling succession plan quota needs this field"),
                new RollingSuccessionPlanQuotaLabelLengthRule(3600),
            });
    }
}
