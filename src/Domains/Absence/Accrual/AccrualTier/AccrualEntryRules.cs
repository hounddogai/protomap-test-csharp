namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual entry must pass. Check returns its problems, or none.</summary>
public interface IAccrualEntryRule
{
    List<string> Check(AccrualEntry item);
}

public sealed record AccrualEntryDiscountRangeRule(double Min, double Max) : IAccrualEntryRule
{
    public List<string> Check(AccrualEntry item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualEntryCodeLengthRule(int MaxLength) : IAccrualEntryRule
{
    public List<string> Check(AccrualEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualEntryRequiredLabelRule(string Message) : IAccrualEntryRule
{
    public List<string> Check(AccrualEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualEntryEmployeeIdLengthRule(int MaxLength) : IAccrualEntryRule
{
    public List<string> Check(AccrualEntry item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualEntryAllowedLabelRule(List<string> Allowed) : IAccrualEntryRule
{
    public List<string> Check(AccrualEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual entry.</summary>
public sealed record AccrualEntryValidator(List<IAccrualEntryRule> Rules)
{
    public List<string> Validate(AccrualEntry item)
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

    public void RequireValid(AccrualEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualEntry> items)
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

/// <summary>Functions of accrual entry rules.</summary>
public static class AccrualEntryRules
{
    public static AccrualEntryValidator DefaultAccrualEntryValidator()
    {
        return new AccrualEntryValidator(
            new List<IAccrualEntryRule>
            {
                new AccrualEntryDiscountRangeRule(5.4, 2259.8),
                new AccrualEntryCodeLengthRule(1000),
                new AccrualEntryRequiredLabelRule("accrual entry needs a value"),
                new AccrualEntryEmployeeIdLengthRule(64),
                new AccrualEntryAllowedLabelRule(new List<string> { "rolling" }),
            });
    }
}
