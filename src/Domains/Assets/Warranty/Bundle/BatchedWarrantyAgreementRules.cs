namespace Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>A rule a batched warranty agreement must pass. Check returns its problems, or none.</summary>
public interface IBatchedWarrantyAgreementRule
{
    List<string> Check(BatchedWarrantyAgreement item);
}

public sealed record BatchedWarrantyAgreementCodeLengthRule(int MaxLength) : IBatchedWarrantyAgreementRule
{
    public List<string> Check(BatchedWarrantyAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyAgreementNameLengthRule(int MaxLength) : IBatchedWarrantyAgreementRule
{
    public List<string> Check(BatchedWarrantyAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyAgreementTitleLengthRule(int MaxLength) : IBatchedWarrantyAgreementRule
{
    public List<string> Check(BatchedWarrantyAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyAgreementCodePrefixRule(string Prefix) : IBatchedWarrantyAgreementRule
{
    public List<string> Check(BatchedWarrantyAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyAgreementLeaveDaysRangeRule(int Min, int Max) : IBatchedWarrantyAgreementRule
{
    public List<string> Check(BatchedWarrantyAgreement item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedWarrantyAgreementLocaleLengthRule(int MaxLength) : IBatchedWarrantyAgreementRule
{
    public List<string> Check(BatchedWarrantyAgreement item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched warranty agreement.</summary>
public sealed record BatchedWarrantyAgreementValidator(List<IBatchedWarrantyAgreementRule> Rules)
{
    public List<string> Validate(BatchedWarrantyAgreement item)
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

    public void RequireValid(BatchedWarrantyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedWarrantyAgreement> items)
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

/// <summary>Functions of batched warranty agreement rules.</summary>
public static class BatchedWarrantyAgreementRules
{
    public static BatchedWarrantyAgreementValidator DefaultBatchedWarrantyAgreementValidator()
    {
        return new BatchedWarrantyAgreementValidator(
            new List<IBatchedWarrantyAgreementRule>
            {
                new BatchedWarrantyAgreementCodeLengthRule(16),
                new BatchedWarrantyAgreementNameLengthRule(16),
                new BatchedWarrantyAgreementTitleLengthRule(32),
                new BatchedWarrantyAgreementCodePrefixRule("REF"),
                new BatchedWarrantyAgreementLeaveDaysRangeRule(1000, 1000),
                new BatchedWarrantyAgreementLocaleLengthRule(1000),
            });
    }
}
