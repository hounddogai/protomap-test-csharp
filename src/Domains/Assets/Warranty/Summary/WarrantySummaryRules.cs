namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty summary must pass. Check returns its problems, or none.</summary>
public interface IWarrantySummaryRule
{
    List<string> Check(WarrantySummary item);
}

public sealed record WarrantySummaryRequiredLabelRule(string Message) : IWarrantySummaryRule
{
    public List<string> Check(WarrantySummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantySummaryRequiredJobCodeRule(string Message) : IWarrantySummaryRule
{
    public List<string> Check(WarrantySummary item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantySummaryEndsAtRangeRule(int Min, int Max) : IWarrantySummaryRule
{
    public List<string> Check(WarrantySummary item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantySummaryCodeLengthRule(int MaxLength) : IWarrantySummaryRule
{
    public List<string> Check(WarrantySummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantySummaryActiveGuardRule(int Limit) : IWarrantySummaryRule
{
    public List<string> Check(WarrantySummary item)
    {
        if (item.Active && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record WarrantySummaryAllowedCostCenterRule(List<string> Allowed) : IWarrantySummaryRule
{
    public List<string> Check(WarrantySummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty summary.</summary>
public sealed record WarrantySummaryValidator(List<IWarrantySummaryRule> Rules)
{
    public List<string> Validate(WarrantySummary item)
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

    public void RequireValid(WarrantySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantySummary> items)
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

/// <summary>Functions of warranty summary rules.</summary>
public static class WarrantySummaryRules
{
    public static WarrantySummaryValidator DefaultWarrantySummaryValidator()
    {
        return new WarrantySummaryValidator(
            new List<IWarrantySummaryRule>
            {
                new WarrantySummaryRequiredLabelRule("warranty summary needs an entry"),
                new WarrantySummaryRequiredJobCodeRule("warranty summary needs this field"),
                new WarrantySummaryEndsAtRangeRule(255, 86400),
                new WarrantySummaryCodeLengthRule(16),
                new WarrantySummaryActiveGuardRule(16),
                new WarrantySummaryAllowedCostCenterRule(new List<string> { "strict", "tiered" }),
            });
    }
}
