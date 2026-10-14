namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending deduction cycle must pass. Check returns its problems, or none.</summary>
public interface IPendingDeductionCycleRule
{
    List<string> Check(PendingDeductionCycle item);
}

public sealed record PendingDeductionCycleLabelLengthRule(int MaxLength) : IPendingDeductionCycleRule
{
    public List<string> Check(PendingDeductionCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionCycleRequiredCodeRule(string Message) : IPendingDeductionCycleRule
{
    public List<string> Check(PendingDeductionCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionCycleDiscountRangeRule(double Min, double Max) : IPendingDeductionCycleRule
{
    public List<string> Check(PendingDeductionCycle item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingDeductionCycleCodePrefixRule(string Prefix) : IPendingDeductionCycleRule
{
    public List<string> Check(PendingDeductionCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingDeductionCycleLimitRangeRule(int Min, int Max) : IPendingDeductionCycleRule
{
    public List<string> Check(PendingDeductionCycle item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingDeductionCycleAllowedCodeRule(List<string> Allowed) : IPendingDeductionCycleRule
{
    public List<string> Check(PendingDeductionCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending deduction cycle.</summary>
public sealed record PendingDeductionCycleValidator(List<IPendingDeductionCycleRule> Rules)
{
    public List<string> Validate(PendingDeductionCycle item)
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

    public void RequireValid(PendingDeductionCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingDeductionCycle> items)
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

/// <summary>Functions of pending deduction cycle rules.</summary>
public static class PendingDeductionCycleRules
{
    public static PendingDeductionCycleValidator DefaultPendingDeductionCycleValidator()
    {
        return new PendingDeductionCycleValidator(
            new List<IPendingDeductionCycleRule>
            {
                new PendingDeductionCycleLabelLengthRule(86400),
                new PendingDeductionCycleRequiredCodeRule("pending deduction cycle needs a value"),
                new PendingDeductionCycleDiscountRangeRule(7.0, 1700.2),
                new PendingDeductionCycleCodePrefixRule("OPS"),
                new PendingDeductionCycleLimitRangeRule(64, 3600),
                new PendingDeductionCycleAllowedCodeRule(new List<string> { "scheduled", "strict", "fallback" }),
            });
    }
}
