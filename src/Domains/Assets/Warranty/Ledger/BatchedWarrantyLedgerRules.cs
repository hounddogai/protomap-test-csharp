namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched warranty ledger must pass. Check returns its problems, or none.</summary>
public interface IBatchedWarrantyLedgerRule
{
    List<string> Check(BatchedWarrantyLedger item);
}

public sealed record BatchedWarrantyLedgerLocationLengthRule(int MaxLength) : IBatchedWarrantyLedgerRule
{
    public List<string> Check(BatchedWarrantyLedger item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyLedgerMinimumRangeRule(double Min, double Max) : IBatchedWarrantyLedgerRule
{
    public List<string> Check(BatchedWarrantyLedger item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyLedgerRequiredLabelRule(string Message) : IBatchedWarrantyLedgerRule
{
    public List<string> Check(BatchedWarrantyLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyLedgerPhoneLengthRule(int MaxLength) : IBatchedWarrantyLedgerRule
{
    public List<string> Check(BatchedWarrantyLedger item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyLedgerAllowedLocationRule(List<string> Allowed) : IBatchedWarrantyLedgerRule
{
    public List<string> Check(BatchedWarrantyLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyLedgerRequiredPhoneRule(string Message) : IBatchedWarrantyLedgerRule
{
    public List<string> Check(BatchedWarrantyLedger item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched warranty ledger.</summary>
public sealed record BatchedWarrantyLedgerValidator(List<IBatchedWarrantyLedgerRule> Rules)
{
    public List<string> Validate(BatchedWarrantyLedger item)
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

    public void RequireValid(BatchedWarrantyLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedWarrantyLedger> items)
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

/// <summary>Functions of batched warranty ledger rules.</summary>
public static class BatchedWarrantyLedgerRules
{
    public static BatchedWarrantyLedgerValidator DefaultBatchedWarrantyLedgerValidator()
    {
        return new BatchedWarrantyLedgerValidator(
            new List<IBatchedWarrantyLedgerRule>
            {
                new BatchedWarrantyLedgerLocationLengthRule(16),
                new BatchedWarrantyLedgerMinimumRangeRule(5.7, 56.2),
                new BatchedWarrantyLedgerRequiredLabelRule("batched warranty ledger needs an entry"),
                new BatchedWarrantyLedgerPhoneLengthRule(86400),
                new BatchedWarrantyLedgerAllowedLocationRule(new List<string>()),
                new BatchedWarrantyLedgerRequiredPhoneRule("batched warranty ledger needs a value"),
            });
    }
}
