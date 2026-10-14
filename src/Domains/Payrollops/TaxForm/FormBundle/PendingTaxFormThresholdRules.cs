namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>A rule a pending tax form threshold must pass. Check returns its problems, or none.</summary>
public interface IPendingTaxFormThresholdRule
{
    List<string> Check(PendingTaxFormThreshold item);
}

public sealed record PendingTaxFormThresholdRequiredExternalRefRule(string Message) : IPendingTaxFormThresholdRule
{
    public List<string> Check(PendingTaxFormThreshold item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTaxFormThresholdRequiredCodeRule(string Message) : IPendingTaxFormThresholdRule
{
    public List<string> Check(PendingTaxFormThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTaxFormThresholdCodeLengthRule(int MaxLength) : IPendingTaxFormThresholdRule
{
    public List<string> Check(PendingTaxFormThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending tax form threshold.</summary>
public sealed record PendingTaxFormThresholdValidator(List<IPendingTaxFormThresholdRule> Rules)
{
    public List<string> Validate(PendingTaxFormThreshold item)
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

    public void RequireValid(PendingTaxFormThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTaxFormThreshold> items)
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

/// <summary>Functions of pending tax form threshold rules.</summary>
public static class PendingTaxFormThresholdRules
{
    public static PendingTaxFormThresholdValidator DefaultPendingTaxFormThresholdValidator()
    {
        return new PendingTaxFormThresholdValidator(
            new List<IPendingTaxFormThresholdRule>
            {
                new PendingTaxFormThresholdRequiredExternalRefRule("pending tax form threshold needs this field"),
                new PendingTaxFormThresholdRequiredCodeRule("pending tax form threshold needs a value"),
                new PendingTaxFormThresholdCodeLengthRule(16),
            });
    }
}
