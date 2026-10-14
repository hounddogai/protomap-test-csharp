namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pay band tier must pass. Check returns its problems, or none.</summary>
public interface IPendingPayBandTierRule
{
    List<string> Check(PendingPayBandTier item);
}

public sealed record PendingPayBandTierScoreRangeRule(double Min, double Max) : IPendingPayBandTierRule
{
    public List<string> Check(PendingPayBandTier item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPayBandTierRequiredCodeRule(string Message) : IPendingPayBandTierRule
{
    public List<string> Check(PendingPayBandTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandTierCeilingRangeRule(double Min, double Max) : IPendingPayBandTierRule
{
    public List<string> Check(PendingPayBandTier item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPayBandTierAllowedLabelRule(List<string> Allowed) : IPendingPayBandTierRule
{
    public List<string> Check(PendingPayBandTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandTierRequiredLabelRule(string Message) : IPendingPayBandTierRule
{
    public List<string> Check(PendingPayBandTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandTierCodeLengthRule(int MaxLength) : IPendingPayBandTierRule
{
    public List<string> Check(PendingPayBandTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pay band tier.</summary>
public sealed record PendingPayBandTierValidator(List<IPendingPayBandTierRule> Rules)
{
    public List<string> Validate(PendingPayBandTier item)
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

    public void RequireValid(PendingPayBandTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayBandTier> items)
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

/// <summary>Functions of pending pay band tier rules.</summary>
public static class PendingPayBandTierRules
{
    public static PendingPayBandTierValidator DefaultPendingPayBandTierValidator()
    {
        return new PendingPayBandTierValidator(
            new List<IPendingPayBandTierRule>
            {
                new PendingPayBandTierScoreRangeRule(4.0, 967.6),
                new PendingPayBandTierRequiredCodeRule("pending pay band tier needs this field"),
                new PendingPayBandTierCeilingRangeRule(1.7, 1763.9),
                new PendingPayBandTierAllowedLabelRule(new List<string>()),
                new PendingPayBandTierRequiredLabelRule("pending pay band tier needs a value"),
                new PendingPayBandTierCodeLengthRule(32),
            });
    }
}
