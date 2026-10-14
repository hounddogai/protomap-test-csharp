namespace Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>A rule a tiered license seat adjustment must pass. Check returns its problems, or none.</summary>
public interface ITieredLicenseSeatAdjustmentRule
{
    List<string> Check(TieredLicenseSeatAdjustment item);
}

public sealed record TieredLicenseSeatAdjustmentActiveGuardRule(double Limit) : ITieredLicenseSeatAdjustmentRule
{
    public List<string> Check(TieredLicenseSeatAdjustment item)
    {
        if (item.Active && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatAdjustmentRequiredSummaryRule(string Message) : ITieredLicenseSeatAdjustmentRule
{
    public List<string> Check(TieredLicenseSeatAdjustment item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatAdjustmentRatingRangeRule(double Min, double Max) : ITieredLicenseSeatAdjustmentRule
{
    public List<string> Check(TieredLicenseSeatAdjustment item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredLicenseSeatAdjustmentSummaryLengthRule(int MaxLength) : ITieredLicenseSeatAdjustmentRule
{
    public List<string> Check(TieredLicenseSeatAdjustment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatAdjustmentMarginRangeRule(double Min, double Max) : ITieredLicenseSeatAdjustmentRule
{
    public List<string> Check(TieredLicenseSeatAdjustment item)
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

/// <summary>Runs every rule of a tiered license seat adjustment.</summary>
public sealed record TieredLicenseSeatAdjustmentValidator(List<ITieredLicenseSeatAdjustmentRule> Rules)
{
    public List<string> Validate(TieredLicenseSeatAdjustment item)
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

    public void RequireValid(TieredLicenseSeatAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLicenseSeatAdjustment> items)
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

/// <summary>Functions of tiered license seat adjustment rules.</summary>
public static class TieredLicenseSeatAdjustmentRules
{
    public static TieredLicenseSeatAdjustmentValidator DefaultTieredLicenseSeatAdjustmentValidator()
    {
        return new TieredLicenseSeatAdjustmentValidator(
            new List<ITieredLicenseSeatAdjustmentRule>
            {
                new TieredLicenseSeatAdjustmentActiveGuardRule(1.5),
                new TieredLicenseSeatAdjustmentRequiredSummaryRule("tiered license seat adjustment needs this field"),
                new TieredLicenseSeatAdjustmentRatingRangeRule(1.8, 1042.0),
                new TieredLicenseSeatAdjustmentSummaryLengthRule(1000),
                new TieredLicenseSeatAdjustmentMarginRangeRule(5.4, 1131.5),
            });
    }
}
