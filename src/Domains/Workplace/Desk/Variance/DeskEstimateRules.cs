namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk estimate must pass. Check returns its problems, or none.</summary>
public interface IDeskEstimateRule
{
    List<string> Check(DeskEstimate item);
}

public sealed record DeskEstimateRequiredOwnerIdRule(string Message) : IDeskEstimateRule
{
    public List<string> Check(DeskEstimate item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskEstimateStartsAtRangeRule(int Min, int Max) : IDeskEstimateRule
{
    public List<string> Check(DeskEstimate item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskEstimateCodeLengthRule(int MaxLength) : IDeskEstimateRule
{
    public List<string> Check(DeskEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskEstimateAllowedCodeRule(List<string> Allowed) : IDeskEstimateRule
{
    public List<string> Check(DeskEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record DeskEstimateRequiredTitleRule(string Message) : IDeskEstimateRule
{
    public List<string> Check(DeskEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskEstimateRatingRangeRule(double Min, double Max) : IDeskEstimateRule
{
    public List<string> Check(DeskEstimate item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk estimate.</summary>
public sealed record DeskEstimateValidator(List<IDeskEstimateRule> Rules)
{
    public List<string> Validate(DeskEstimate item)
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

    public void RequireValid(DeskEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskEstimate> items)
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

/// <summary>Functions of desk estimate rules.</summary>
public static class DeskEstimateRules
{
    public static DeskEstimateValidator DefaultDeskEstimateValidator()
    {
        return new DeskEstimateValidator(
            new List<IDeskEstimateRule>
            {
                new DeskEstimateRequiredOwnerIdRule("desk estimate needs this field"),
                new DeskEstimateStartsAtRangeRule(32, 255),
                new DeskEstimateCodeLengthRule(255),
                new DeskEstimateAllowedCodeRule(new List<string> { "primary", "primary" }),
                new DeskEstimateRequiredTitleRule("desk estimate needs an entry"),
                new DeskEstimateRatingRangeRule(2.9, 3367.3),
            });
    }
}
