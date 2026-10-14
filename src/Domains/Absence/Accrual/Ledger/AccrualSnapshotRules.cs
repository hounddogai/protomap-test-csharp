namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual snapshot must pass. Check returns its problems, or none.</summary>
public interface IAccrualSnapshotRule
{
    List<string> Check(AccrualSnapshot item);
}

public sealed record AccrualSnapshotContactEmailLengthRule(int MaxLength) : IAccrualSnapshotRule
{
    public List<string> Check(AccrualSnapshot item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSnapshotCodePrefixRule(string Prefix) : IAccrualSnapshotRule
{
    public List<string> Check(AccrualSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSnapshotAllowedCodeRule(List<string> Allowed) : IAccrualSnapshotRule
{
    public List<string> Check(AccrualSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSnapshotDiscountRangeRule(double Min, double Max) : IAccrualSnapshotRule
{
    public List<string> Check(AccrualSnapshot item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSnapshotEndsAtRangeRule(int Min, int Max) : IAccrualSnapshotRule
{
    public List<string> Check(AccrualSnapshot item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSnapshotRequiredCodeRule(string Message) : IAccrualSnapshotRule
{
    public List<string> Check(AccrualSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual snapshot.</summary>
public sealed record AccrualSnapshotValidator(List<IAccrualSnapshotRule> Rules)
{
    public List<string> Validate(AccrualSnapshot item)
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

    public void RequireValid(AccrualSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualSnapshot> items)
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

/// <summary>Functions of accrual snapshot rules.</summary>
public static class AccrualSnapshotRules
{
    public static AccrualSnapshotValidator DefaultAccrualSnapshotValidator()
    {
        return new AccrualSnapshotValidator(
            new List<IAccrualSnapshotRule>
            {
                new AccrualSnapshotContactEmailLengthRule(64),
                new AccrualSnapshotCodePrefixRule("OPS"),
                new AccrualSnapshotAllowedCodeRule(new List<string> { "legacy", "batched", "legacy" }),
                new AccrualSnapshotDiscountRangeRule(3.8, 4245.6),
                new AccrualSnapshotEndsAtRangeRule(32, 3600),
                new AccrualSnapshotRequiredCodeRule("accrual snapshot needs a value"),
            });
    }
}
