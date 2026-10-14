namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pay band group must pass. Check returns its problems, or none.</summary>
public interface IPendingPayBandGroupRule
{
    List<string> Check(PendingPayBandGroup item);
}

public sealed record PendingPayBandGroupWeightRangeRule(double Min, double Max) : IPendingPayBandGroupRule
{
    public List<string> Check(PendingPayBandGroup item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPayBandGroupCodePrefixRule(string Prefix) : IPendingPayBandGroupRule
{
    public List<string> Check(PendingPayBandGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandGroupAllowedLabelRule(List<string> Allowed) : IPendingPayBandGroupRule
{
    public List<string> Check(PendingPayBandGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandGroupRequiredLabelRule(string Message) : IPendingPayBandGroupRule
{
    public List<string> Check(PendingPayBandGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandGroupRequiredCodeRule(string Message) : IPendingPayBandGroupRule
{
    public List<string> Check(PendingPayBandGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandGroupLabelLengthRule(int MaxLength) : IPendingPayBandGroupRule
{
    public List<string> Check(PendingPayBandGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pay band group.</summary>
public sealed record PendingPayBandGroupValidator(List<IPendingPayBandGroupRule> Rules)
{
    public List<string> Validate(PendingPayBandGroup item)
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

    public void RequireValid(PendingPayBandGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayBandGroup> items)
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

/// <summary>Functions of pending pay band group rules.</summary>
public static class PendingPayBandGroupRules
{
    public static PendingPayBandGroupValidator DefaultPendingPayBandGroupValidator()
    {
        return new PendingPayBandGroupValidator(
            new List<IPendingPayBandGroupRule>
            {
                new PendingPayBandGroupWeightRangeRule(1.4, 3681.8),
                new PendingPayBandGroupCodePrefixRule("OPS"),
                new PendingPayBandGroupAllowedLabelRule(new List<string> { "adaptive", "rolling", "strict" }),
                new PendingPayBandGroupRequiredLabelRule("pending pay band group needs an entry"),
                new PendingPayBandGroupRequiredCodeRule("pending pay band group needs a value"),
                new PendingPayBandGroupLabelLengthRule(86400),
            });
    }
}
