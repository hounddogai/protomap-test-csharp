namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

/// <summary>A rule a position reservation must pass. Check returns its problems, or none.</summary>
public interface IPositionReservationRule
{
    List<string> Check(PositionReservation item);
}

public sealed record PositionReservationCostCenterLengthRule(int MaxLength) : IPositionReservationRule
{
    public List<string> Check(PositionReservation item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionReservationMarginRangeRule(double Min, double Max) : IPositionReservationRule
{
    public List<string> Check(PositionReservation item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionReservationRatioRangeRule(double Min, double Max) : IPositionReservationRule
{
    public List<string> Check(PositionReservation item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionReservationCodePrefixRule(string Prefix) : IPositionReservationRule
{
    public List<string> Check(PositionReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionReservationFteRangeRule(double Min, double Max) : IPositionReservationRule
{
    public List<string> Check(PositionReservation item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position reservation.</summary>
public sealed record PositionReservationValidator(List<IPositionReservationRule> Rules)
{
    public List<string> Validate(PositionReservation item)
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

    public void RequireValid(PositionReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionReservation> items)
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

/// <summary>Functions of position reservation rules.</summary>
public static class PositionReservationRules
{
    public static PositionReservationValidator DefaultPositionReservationValidator()
    {
        return new PositionReservationValidator(
            new List<IPositionReservationRule>
            {
                new PositionReservationCostCenterLengthRule(86400),
                new PositionReservationMarginRangeRule(3.8, 1392.0),
                new PositionReservationRatioRangeRule(5.4, 4885.1),
                new PositionReservationCodePrefixRule("ACM"),
                new PositionReservationFteRangeRule(6.4, 4767.8),
            });
    }
}
