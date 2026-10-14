namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band plan must pass. Check returns its problems, or none.</summary>
public interface IPayBandPlanRule
{
    List<string> Check(PayBandPlan item);
}

public sealed record PayBandPlanBatchSizeRangeRule(int Min, int Max) : IPayBandPlanRule
{
    public List<string> Check(PayBandPlan item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandPlanChecksumLengthRule(int MaxLength) : IPayBandPlanRule
{
    public List<string> Check(PayBandPlan item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandPlanAllowedLabelRule(List<string> Allowed) : IPayBandPlanRule
{
    public List<string> Check(PayBandPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PayBandPlanCodePrefixRule(string Prefix) : IPayBandPlanRule
{
    public List<string> Check(PayBandPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandPlanTimezoneLengthRule(int MaxLength) : IPayBandPlanRule
{
    public List<string> Check(PayBandPlan item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band plan.</summary>
public sealed record PayBandPlanValidator(List<IPayBandPlanRule> Rules)
{
    public List<string> Validate(PayBandPlan item)
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

    public void RequireValid(PayBandPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandPlan> items)
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

/// <summary>Functions of pay band plan rules.</summary>
public static class PayBandPlanRules
{
    public static PayBandPlanValidator DefaultPayBandPlanValidator()
    {
        return new PayBandPlanValidator(
            new List<IPayBandPlanRule>
            {
                new PayBandPlanBatchSizeRangeRule(16, 64),
                new PayBandPlanChecksumLengthRule(32),
                new PayBandPlanAllowedLabelRule(new List<string> { "batched", "batched" }),
                new PayBandPlanCodePrefixRule("OPS"),
                new PayBandPlanTimezoneLengthRule(86400),
            });
    }
}
