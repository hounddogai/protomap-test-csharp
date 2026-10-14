namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>A rule a legacy retro pay summary must pass. Check returns its problems, or none.</summary>
public interface ILegacyRetroPaySummaryRule
{
    List<string> Check(LegacyRetroPaySummary item);
}

public sealed record LegacyRetroPaySummaryMaximumRangeRule(double Min, double Max) : ILegacyRetroPaySummaryRule
{
    public List<string> Check(LegacyRetroPaySummary item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyRetroPaySummaryApprovedGuardRule(int Limit) : ILegacyRetroPaySummaryRule
{
    public List<string> Check(LegacyRetroPaySummary item)
    {
        if (item.Approved && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySummaryQuantityRangeRule(int Min, int Max) : ILegacyRetroPaySummaryRule
{
    public List<string> Check(LegacyRetroPaySummary item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyRetroPaySummaryRequiredCodeRule(string Message) : ILegacyRetroPaySummaryRule
{
    public List<string> Check(LegacyRetroPaySummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySummaryExternalRefLengthRule(int MaxLength) : ILegacyRetroPaySummaryRule
{
    public List<string> Check(LegacyRetroPaySummary item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPaySummaryCodePrefixRule(string Prefix) : ILegacyRetroPaySummaryRule
{
    public List<string> Check(LegacyRetroPaySummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy retro pay summary.</summary>
public sealed record LegacyRetroPaySummaryValidator(List<ILegacyRetroPaySummaryRule> Rules)
{
    public List<string> Validate(LegacyRetroPaySummary item)
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

    public void RequireValid(LegacyRetroPaySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRetroPaySummary> items)
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

/// <summary>Functions of legacy retro pay summary rules.</summary>
public static class LegacyRetroPaySummaryRules
{
    public static LegacyRetroPaySummaryValidator DefaultLegacyRetroPaySummaryValidator()
    {
        return new LegacyRetroPaySummaryValidator(
            new List<ILegacyRetroPaySummaryRule>
            {
                new LegacyRetroPaySummaryMaximumRangeRule(6.2, 651.8),
                new LegacyRetroPaySummaryApprovedGuardRule(32),
                new LegacyRetroPaySummaryQuantityRangeRule(255, 86400),
                new LegacyRetroPaySummaryRequiredCodeRule("legacy retro pay summary needs this field"),
                new LegacyRetroPaySummaryExternalRefLengthRule(120),
                new LegacyRetroPaySummaryCodePrefixRule("X"),
            });
    }
}
