namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted access badge reservation must pass. Check returns its problems, or none.</summary>
public interface IWeightedAccessBadgeReservationRule
{
    List<string> Check(WeightedAccessBadgeReservation item);
}

public sealed record WeightedAccessBadgeReservationTitleLengthRule(int MaxLength) : IWeightedAccessBadgeReservationRule
{
    public List<string> Check(WeightedAccessBadgeReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeReservationCostCenterLengthRule(int MaxLength) : IWeightedAccessBadgeReservationRule
{
    public List<string> Check(WeightedAccessBadgeReservation item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeReservationAllowedGradeRule(List<string> Allowed) : IWeightedAccessBadgeReservationRule
{
    public List<string> Check(WeightedAccessBadgeReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is too long" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeReservationRequiredTitleRule(string Message) : IWeightedAccessBadgeReservationRule
{
    public List<string> Check(WeightedAccessBadgeReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAccessBadgeReservationCeilingRangeRule(double Min, double Max) : IWeightedAccessBadgeReservationRule
{
    public List<string> Check(WeightedAccessBadgeReservation item)
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

/// <summary>Runs every rule of a weighted access badge reservation.</summary>
public sealed record WeightedAccessBadgeReservationValidator(List<IWeightedAccessBadgeReservationRule> Rules)
{
    public List<string> Validate(WeightedAccessBadgeReservation item)
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

    public void RequireValid(WeightedAccessBadgeReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAccessBadgeReservation> items)
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

/// <summary>Functions of weighted access badge reservation rules.</summary>
public static class WeightedAccessBadgeReservationRules
{
    public static WeightedAccessBadgeReservationValidator DefaultWeightedAccessBadgeReservationValidator()
    {
        return new WeightedAccessBadgeReservationValidator(
            new List<IWeightedAccessBadgeReservationRule>
            {
                new WeightedAccessBadgeReservationTitleLengthRule(32),
                new WeightedAccessBadgeReservationCostCenterLengthRule(32),
                new WeightedAccessBadgeReservationAllowedGradeRule(new List<string> { "adaptive", "regional" }),
                new WeightedAccessBadgeReservationRequiredTitleRule(
                    "weighted access badge reservation needs an entry"),
                new WeightedAccessBadgeReservationCeilingRangeRule(3.1, 4929.5),
            });
    }
}
