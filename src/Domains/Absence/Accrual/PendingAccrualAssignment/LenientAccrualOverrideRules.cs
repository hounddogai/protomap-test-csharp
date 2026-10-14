namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>A rule a lenient accrual override must pass. Check returns its problems, or none.</summary>
public interface ILenientAccrualOverrideRule
{
    List<string> Check(LenientAccrualOverride item);
}

public sealed record LenientAccrualOverrideRegionLengthRule(int MaxLength) : ILenientAccrualOverrideRule
{
    public List<string> Check(LenientAccrualOverride item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualOverrideRequiredLabelRule(string Message) : ILenientAccrualOverrideRule
{
    public List<string> Check(LenientAccrualOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualOverrideRequiredOwnerIdRule(string Message) : ILenientAccrualOverrideRule
{
    public List<string> Check(LenientAccrualOverride item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAccrualOverrideCodePrefixRule(string Prefix) : ILenientAccrualOverrideRule
{
    public List<string> Check(LenientAccrualOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient accrual override.</summary>
public sealed record LenientAccrualOverrideValidator(List<ILenientAccrualOverrideRule> Rules)
{
    public List<string> Validate(LenientAccrualOverride item)
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

    public void RequireValid(LenientAccrualOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAccrualOverride> items)
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

/// <summary>Functions of lenient accrual override rules.</summary>
public static class LenientAccrualOverrideRules
{
    public static LenientAccrualOverrideValidator DefaultLenientAccrualOverrideValidator()
    {
        return new LenientAccrualOverrideValidator(
            new List<ILenientAccrualOverrideRule>
            {
                new LenientAccrualOverrideRegionLengthRule(3600),
                new LenientAccrualOverrideRequiredLabelRule("lenient accrual override needs an entry"),
                new LenientAccrualOverrideRequiredOwnerIdRule("lenient accrual override needs this field"),
                new LenientAccrualOverrideCodePrefixRule("ACM"),
            });
    }
}
