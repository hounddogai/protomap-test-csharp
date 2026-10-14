namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>A rule a strict headcount reservation must pass. Check returns its problems, or none.</summary>
public interface IStrictHeadcountReservationRule
{
    List<string> Check(StrictHeadcountReservation item);
}

public sealed record StrictHeadcountReservationDiscountRangeRule(double Min, double Max) : IStrictHeadcountReservationRule
{
    public List<string> Check(StrictHeadcountReservation item)
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

public sealed record StrictHeadcountReservationCostRangeRule(double Min, double Max) : IStrictHeadcountReservationRule
{
    public List<string> Check(StrictHeadcountReservation item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictHeadcountReservationCodePrefixRule(string Prefix) : IStrictHeadcountReservationRule
{
    public List<string> Check(StrictHeadcountReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict headcount reservation.</summary>
public sealed record StrictHeadcountReservationValidator(List<IStrictHeadcountReservationRule> Rules)
{
    public List<string> Validate(StrictHeadcountReservation item)
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

    public void RequireValid(StrictHeadcountReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictHeadcountReservation> items)
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

/// <summary>Functions of strict headcount reservation rules.</summary>
public static class StrictHeadcountReservationRules
{
    public static StrictHeadcountReservationValidator DefaultStrictHeadcountReservationValidator()
    {
        return new StrictHeadcountReservationValidator(
            new List<IStrictHeadcountReservationRule>
            {
                new StrictHeadcountReservationDiscountRangeRule(7.5, 1997.8),
                new StrictHeadcountReservationCostRangeRule(2.6, 4222.2),
                new StrictHeadcountReservationCodePrefixRule("ACM"),
            });
    }
}
