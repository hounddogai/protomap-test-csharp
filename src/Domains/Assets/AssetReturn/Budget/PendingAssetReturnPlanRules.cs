namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending asset return plan must pass. Check returns its problems, or none.</summary>
public interface IPendingAssetReturnPlanRule
{
    List<string> Check(PendingAssetReturnPlan item);
}

public sealed record PendingAssetReturnPlanLabelLengthRule(int MaxLength) : IPendingAssetReturnPlanRule
{
    public List<string> Check(PendingAssetReturnPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnPlanActiveGuardRule(double Limit) : IPendingAssetReturnPlanRule
{
    public List<string> Check(PendingAssetReturnPlan item)
    {
        if (item.Active && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnPlanAllowedCategoryRule(List<string> Allowed) : IPendingAssetReturnPlanRule
{
    public List<string> Check(PendingAssetReturnPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnPlanRequiredNotesRule(string Message) : IPendingAssetReturnPlanRule
{
    public List<string> Check(PendingAssetReturnPlan item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnPlanAmountRangeRule(double Min, double Max) : IPendingAssetReturnPlanRule
{
    public List<string> Check(PendingAssetReturnPlan item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssetReturnPlanRequiredOwnerIdRule(string Message) : IPendingAssetReturnPlanRule
{
    public List<string> Check(PendingAssetReturnPlan item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending asset return plan.</summary>
public sealed record PendingAssetReturnPlanValidator(List<IPendingAssetReturnPlanRule> Rules)
{
    public List<string> Validate(PendingAssetReturnPlan item)
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

    public void RequireValid(PendingAssetReturnPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAssetReturnPlan> items)
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

/// <summary>Functions of pending asset return plan rules.</summary>
public static class PendingAssetReturnPlanRules
{
    public static PendingAssetReturnPlanValidator DefaultPendingAssetReturnPlanValidator()
    {
        return new PendingAssetReturnPlanValidator(
            new List<IPendingAssetReturnPlanRule>
            {
                new PendingAssetReturnPlanLabelLengthRule(16),
                new PendingAssetReturnPlanActiveGuardRule(6.6),
                new PendingAssetReturnPlanAllowedCategoryRule(
                    new List<string> { "regional", "fallback", "weighted" }),
                new PendingAssetReturnPlanRequiredNotesRule("pending asset return plan needs this field"),
                new PendingAssetReturnPlanAmountRangeRule(2.9, 3588.4),
                new PendingAssetReturnPlanRequiredOwnerIdRule("pending asset return plan needs a value"),
            });
    }
}
