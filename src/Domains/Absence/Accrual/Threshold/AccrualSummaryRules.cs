namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual summary must pass. Check returns its problems, or none.</summary>
public interface IAccrualSummaryRule
{
    List<string> Check(AccrualSummary item);
}

public sealed record AccrualSummaryRequiredCodeRule(string Message) : IAccrualSummaryRule
{
    public List<string> Check(AccrualSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualSummaryRequiredSkillRule(string Message) : IAccrualSummaryRule
{
    public List<string> Check(AccrualSummary item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualSummaryAllowedSkillRule(List<string> Allowed) : IAccrualSummaryRule
{
    public List<string> Check(AccrualSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSummaryCodePrefixRule(string Prefix) : IAccrualSummaryRule
{
    public List<string> Check(AccrualSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSummaryRequiredNameRule(string Message) : IAccrualSummaryRule
{
    public List<string> Check(AccrualSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualSummaryVisibleGuardRule(int Limit) : IAccrualSummaryRule
{
    public List<string> Check(AccrualSummary item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual summary.</summary>
public sealed record AccrualSummaryValidator(List<IAccrualSummaryRule> Rules)
{
    public List<string> Validate(AccrualSummary item)
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

    public void RequireValid(AccrualSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualSummary> items)
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

/// <summary>Functions of accrual summary rules.</summary>
public static class AccrualSummaryRules
{
    public static AccrualSummaryValidator DefaultAccrualSummaryValidator()
    {
        return new AccrualSummaryValidator(
            new List<IAccrualSummaryRule>
            {
                new AccrualSummaryRequiredCodeRule("accrual summary needs a value"),
                new AccrualSummaryRequiredSkillRule("accrual summary needs a value"),
                new AccrualSummaryAllowedSkillRule(new List<string> { "scheduled", "regional", "capped" }),
                new AccrualSummaryCodePrefixRule("REF"),
                new AccrualSummaryRequiredNameRule("accrual summary needs this field"),
                new AccrualSummaryVisibleGuardRule(255),
            });
    }
}
