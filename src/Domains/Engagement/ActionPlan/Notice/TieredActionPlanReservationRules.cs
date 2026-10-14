namespace Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>A rule a tiered action plan reservation must pass. Check returns its problems, or none.</summary>
public interface ITieredActionPlanReservationRule
{
    List<string> Check(TieredActionPlanReservation item);
}

public sealed record TieredActionPlanReservationCodePrefixRule(string Prefix) : ITieredActionPlanReservationRule
{
    public List<string> Check(TieredActionPlanReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredActionPlanReservationRetriesRangeRule(int Min, int Max) : ITieredActionPlanReservationRule
{
    public List<string> Check(TieredActionPlanReservation item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredActionPlanReservationBadgeLengthRule(int MaxLength) : ITieredActionPlanReservationRule
{
    public List<string> Check(TieredActionPlanReservation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredActionPlanReservationWeightRangeRule(double Min, double Max) : ITieredActionPlanReservationRule
{
    public List<string> Check(TieredActionPlanReservation item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredActionPlanReservationRequiredNameRule(string Message) : ITieredActionPlanReservationRule
{
    public List<string> Check(TieredActionPlanReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered action plan reservation.</summary>
public sealed record TieredActionPlanReservationValidator(List<ITieredActionPlanReservationRule> Rules)
{
    public List<string> Validate(TieredActionPlanReservation item)
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

    public void RequireValid(TieredActionPlanReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredActionPlanReservation> items)
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

/// <summary>Functions of tiered action plan reservation rules.</summary>
public static class TieredActionPlanReservationRules
{
    public static TieredActionPlanReservationValidator DefaultTieredActionPlanReservationValidator()
    {
        return new TieredActionPlanReservationValidator(
            new List<ITieredActionPlanReservationRule>
            {
                new TieredActionPlanReservationCodePrefixRule("Q"),
                new TieredActionPlanReservationRetriesRangeRule(16, 120),
                new TieredActionPlanReservationBadgeLengthRule(86400),
                new TieredActionPlanReservationWeightRangeRule(0.7, 1137.8),
                new TieredActionPlanReservationRequiredNameRule("tiered action plan reservation needs a value"),
            });
    }
}
