namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position summary must pass. Check returns its problems, or none.</summary>
public interface IPositionSummaryRule
{
    List<string> Check(PositionSummary item);
}

public sealed record PositionSummaryEndsAtRangeRule(int Min, int Max) : IPositionSummaryRule
{
    public List<string> Check(PositionSummary item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionSummaryAllowedCodeRule(List<string> Allowed) : IPositionSummaryRule
{
    public List<string> Check(PositionSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PositionSummaryRequiredDescriptionRule(string Message) : IPositionSummaryRule
{
    public List<string> Check(PositionSummary item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionSummaryPhoneLengthRule(int MaxLength) : IPositionSummaryRule
{
    public List<string> Check(PositionSummary item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionSummaryDiscountRangeRule(double Min, double Max) : IPositionSummaryRule
{
    public List<string> Check(PositionSummary item)
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

/// <summary>Runs every rule of a position summary.</summary>
public sealed record PositionSummaryValidator(List<IPositionSummaryRule> Rules)
{
    public List<string> Validate(PositionSummary item)
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

    public void RequireValid(PositionSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionSummary> items)
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

/// <summary>Functions of position summary rules.</summary>
public static class PositionSummaryRules
{
    public static PositionSummaryValidator DefaultPositionSummaryValidator()
    {
        return new PositionSummaryValidator(
            new List<IPositionSummaryRule>
            {
                new PositionSummaryEndsAtRangeRule(255, 3600),
                new PositionSummaryAllowedCodeRule(new List<string>()),
                new PositionSummaryRequiredDescriptionRule("position summary needs this field"),
                new PositionSummaryPhoneLengthRule(3600),
                new PositionSummaryDiscountRangeRule(8.2, 4031.4),
            });
    }
}
