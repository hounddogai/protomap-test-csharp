namespace Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>A rule a weighted refresh reservation must pass. Check returns its problems, or none.</summary>
public interface IWeightedRefreshReservationRule
{
    List<string> Check(WeightedRefreshReservation item);
}

public sealed record WeightedRefreshReservationExternalRefLengthRule(int MaxLength) : IWeightedRefreshReservationRule
{
    public List<string> Check(WeightedRefreshReservation item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRefreshReservationAmountRangeRule(double Min, double Max) : IWeightedRefreshReservationRule
{
    public List<string> Check(WeightedRefreshReservation item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRefreshReservationLabelLengthRule(int MaxLength) : IWeightedRefreshReservationRule
{
    public List<string> Check(WeightedRefreshReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted refresh reservation.</summary>
public sealed record WeightedRefreshReservationValidator(List<IWeightedRefreshReservationRule> Rules)
{
    public List<string> Validate(WeightedRefreshReservation item)
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

    public void RequireValid(WeightedRefreshReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRefreshReservation> items)
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

/// <summary>Functions of weighted refresh reservation rules.</summary>
public static class WeightedRefreshReservationRules
{
    public static WeightedRefreshReservationValidator DefaultWeightedRefreshReservationValidator()
    {
        return new WeightedRefreshReservationValidator(
            new List<IWeightedRefreshReservationRule>
            {
                new WeightedRefreshReservationExternalRefLengthRule(80),
                new WeightedRefreshReservationAmountRangeRule(3.1, 935.1),
                new WeightedRefreshReservationLabelLengthRule(32),
            });
    }
}
