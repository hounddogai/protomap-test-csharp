namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending warranty group must pass. Check returns its problems, or none.</summary>
public interface IPendingWarrantyGroupRule
{
    List<string> Check(PendingWarrantyGroup item);
}

public sealed record PendingWarrantyGroupTitleLengthRule(int MaxLength) : IPendingWarrantyGroupRule
{
    public List<string> Check(PendingWarrantyGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyGroupLabelLengthRule(int MaxLength) : IPendingWarrantyGroupRule
{
    public List<string> Check(PendingWarrantyGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyGroupAmountRangeRule(double Min, double Max) : IPendingWarrantyGroupRule
{
    public List<string> Check(PendingWarrantyGroup item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingWarrantyGroupRatioRangeRule(double Min, double Max) : IPendingWarrantyGroupRule
{
    public List<string> Check(PendingWarrantyGroup item)
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

public sealed record PendingWarrantyGroupAllowedLabelRule(List<string> Allowed) : IPendingWarrantyGroupRule
{
    public List<string> Check(PendingWarrantyGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PendingWarrantyGroupRetriesRangeRule(int Min, int Max) : IPendingWarrantyGroupRule
{
    public List<string> Check(PendingWarrantyGroup item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending warranty group.</summary>
public sealed record PendingWarrantyGroupValidator(List<IPendingWarrantyGroupRule> Rules)
{
    public List<string> Validate(PendingWarrantyGroup item)
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

    public void RequireValid(PendingWarrantyGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingWarrantyGroup> items)
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

/// <summary>Functions of pending warranty group rules.</summary>
public static class PendingWarrantyGroupRules
{
    public static PendingWarrantyGroupValidator DefaultPendingWarrantyGroupValidator()
    {
        return new PendingWarrantyGroupValidator(
            new List<IPendingWarrantyGroupRule>
            {
                new PendingWarrantyGroupTitleLengthRule(64),
                new PendingWarrantyGroupLabelLengthRule(86400),
                new PendingWarrantyGroupAmountRangeRule(6.6, 3938.1),
                new PendingWarrantyGroupRatioRangeRule(2.3, 3052.0),
                new PendingWarrantyGroupAllowedLabelRule(new List<string>()),
                new PendingWarrantyGroupRetriesRangeRule(3600, 86400),
            });
    }
}
