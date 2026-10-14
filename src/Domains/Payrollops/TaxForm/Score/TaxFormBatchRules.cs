namespace Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>A rule a tax form batch must pass. Check returns its problems, or none.</summary>
public interface ITaxFormBatchRule
{
    List<string> Check(TaxFormBatch item);
}

public sealed record TaxFormBatchCodeLengthRule(int MaxLength) : ITaxFormBatchRule
{
    public List<string> Check(TaxFormBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBatchPinnedGuardRule(int Limit) : ITaxFormBatchRule
{
    public List<string> Check(TaxFormBatch item)
    {
        if (item.Pinned && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBatchRequiredLabelRule(string Message) : ITaxFormBatchRule
{
    public List<string> Check(TaxFormBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBatchEndsAtRangeRule(int Min, int Max) : ITaxFormBatchRule
{
    public List<string> Check(TaxFormBatch item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBatchRequiredSummaryRule(string Message) : ITaxFormBatchRule
{
    public List<string> Check(TaxFormBatch item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form batch.</summary>
public sealed record TaxFormBatchValidator(List<ITaxFormBatchRule> Rules)
{
    public List<string> Validate(TaxFormBatch item)
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

    public void RequireValid(TaxFormBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormBatch> items)
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

/// <summary>Functions of tax form batch rules.</summary>
public static class TaxFormBatchRules
{
    public static TaxFormBatchValidator DefaultTaxFormBatchValidator()
    {
        return new TaxFormBatchValidator(
            new List<ITaxFormBatchRule>
            {
                new TaxFormBatchCodeLengthRule(64),
                new TaxFormBatchPinnedGuardRule(1000),
                new TaxFormBatchRequiredLabelRule("tax form batch needs an entry"),
                new TaxFormBatchEndsAtRangeRule(16, 80),
                new TaxFormBatchRequiredSummaryRule("tax form batch needs a value"),
            });
    }
}
