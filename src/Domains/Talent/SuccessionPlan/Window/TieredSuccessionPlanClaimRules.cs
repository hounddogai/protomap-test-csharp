namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered succession plan claim must pass. Check returns its problems, or none.</summary>
public interface ITieredSuccessionPlanClaimRule
{
    List<string> Check(TieredSuccessionPlanClaim item);
}

public sealed record TieredSuccessionPlanClaimRequiredCodeRule(string Message) : ITieredSuccessionPlanClaimRule
{
    public List<string> Check(TieredSuccessionPlanClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSuccessionPlanClaimAllowedOwnerIdRule(List<string> Allowed) : ITieredSuccessionPlanClaimRule
{
    public List<string> Check(TieredSuccessionPlanClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is stale" };
        }
        return new List<string>();
    }
}

public sealed record TieredSuccessionPlanClaimUpdatedAtRangeRule(int Min, int Max) : ITieredSuccessionPlanClaimRule
{
    public List<string> Check(TieredSuccessionPlanClaim item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSuccessionPlanClaimPhoneLengthRule(int MaxLength) : ITieredSuccessionPlanClaimRule
{
    public List<string> Check(TieredSuccessionPlanClaim item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSuccessionPlanClaimTitleLengthRule(int MaxLength) : ITieredSuccessionPlanClaimRule
{
    public List<string> Check(TieredSuccessionPlanClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered succession plan claim.</summary>
public sealed record TieredSuccessionPlanClaimValidator(List<ITieredSuccessionPlanClaimRule> Rules)
{
    public List<string> Validate(TieredSuccessionPlanClaim item)
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

    public void RequireValid(TieredSuccessionPlanClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSuccessionPlanClaim> items)
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

/// <summary>Functions of tiered succession plan claim rules.</summary>
public static class TieredSuccessionPlanClaimRules
{
    public static TieredSuccessionPlanClaimValidator DefaultTieredSuccessionPlanClaimValidator()
    {
        return new TieredSuccessionPlanClaimValidator(
            new List<ITieredSuccessionPlanClaimRule>
            {
                new TieredSuccessionPlanClaimRequiredCodeRule("tiered succession plan claim needs an entry"),
                new TieredSuccessionPlanClaimAllowedOwnerIdRule(new List<string> { "regional" }),
                new TieredSuccessionPlanClaimUpdatedAtRangeRule(1000, 3600),
                new TieredSuccessionPlanClaimPhoneLengthRule(64),
                new TieredSuccessionPlanClaimTitleLengthRule(80),
            });
    }
}
