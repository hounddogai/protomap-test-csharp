namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual tier must pass. Check returns its problems, or none.</summary>
public interface IAccrualTierRule
{
    List<string> Check(AccrualTier item);
}

public sealed record AccrualTierDescriptionLengthRule(int MaxLength) : IAccrualTierRule
{
    public List<string> Check(AccrualTier item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualTierRequiredLabelRule(string Message) : IAccrualTierRule
{
    public List<string> Check(AccrualTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualTierLabelLengthRule(int MaxLength) : IAccrualTierRule
{
    public List<string> Check(AccrualTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualTierAllowedLocationRule(List<string> Allowed) : IAccrualTierRule
{
    public List<string> Check(AccrualTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual tier.</summary>
public sealed record AccrualTierValidator(List<IAccrualTierRule> Rules)
{
    public List<string> Validate(AccrualTier item)
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

    public void RequireValid(AccrualTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualTier> items)
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

/// <summary>Functions of accrual tier rules.</summary>
public static class AccrualTierRules
{
    public static AccrualTierValidator DefaultAccrualTierValidator()
    {
        return new AccrualTierValidator(
            new List<IAccrualTierRule>
            {
                new AccrualTierDescriptionLengthRule(120),
                new AccrualTierRequiredLabelRule("accrual tier needs a value"),
                new AccrualTierLabelLengthRule(3600),
                new AccrualTierAllowedLocationRule(new List<string>()),
            });
    }
}
