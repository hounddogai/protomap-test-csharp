namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted booking variance must pass. Check returns its problems, or none.</summary>
public interface IWeightedBookingVarianceRule
{
    List<string> Check(WeightedBookingVariance item);
}

public sealed record WeightedBookingVariancePriorityRangeRule(int Min, int Max) : IWeightedBookingVarianceRule
{
    public List<string> Check(WeightedBookingVariance item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedBookingVarianceAllowedCodeRule(List<string> Allowed) : IWeightedBookingVarianceRule
{
    public List<string> Check(WeightedBookingVariance item)
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

public sealed record WeightedBookingVarianceRequiredNameRule(string Message) : IWeightedBookingVarianceRule
{
    public List<string> Check(WeightedBookingVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBookingVarianceRequiredCodeRule(string Message) : IWeightedBookingVarianceRule
{
    public List<string> Check(WeightedBookingVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBookingVarianceMaximumRangeRule(double Min, double Max) : IWeightedBookingVarianceRule
{
    public List<string> Check(WeightedBookingVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBookingVarianceRequiredChannelRule(string Message) : IWeightedBookingVarianceRule
{
    public List<string> Check(WeightedBookingVariance item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted booking variance.</summary>
public sealed record WeightedBookingVarianceValidator(List<IWeightedBookingVarianceRule> Rules)
{
    public List<string> Validate(WeightedBookingVariance item)
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

    public void RequireValid(WeightedBookingVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBookingVariance> items)
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

/// <summary>Functions of weighted booking variance rules.</summary>
public static class WeightedBookingVarianceRules
{
    public static WeightedBookingVarianceValidator DefaultWeightedBookingVarianceValidator()
    {
        return new WeightedBookingVarianceValidator(
            new List<IWeightedBookingVarianceRule>
            {
                new WeightedBookingVariancePriorityRangeRule(32, 255),
                new WeightedBookingVarianceAllowedCodeRule(new List<string> { "legacy", "tiered", "adaptive" }),
                new WeightedBookingVarianceRequiredNameRule("weighted booking variance needs an entry"),
                new WeightedBookingVarianceRequiredCodeRule("weighted booking variance needs an entry"),
                new WeightedBookingVarianceMaximumRangeRule(10.0, 4488.5),
                new WeightedBookingVarianceRequiredChannelRule("weighted booking variance needs an entry"),
            });
    }
}
