namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>A rule a floor plan reservation must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanReservationRule
{
    List<string> Check(FloorPlanReservation item);
}

public sealed record FloorPlanReservationContactEmailLengthRule(int MaxLength) : IFloorPlanReservationRule
{
    public List<string> Check(FloorPlanReservation item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanReservationDescriptionLengthRule(int MaxLength) : IFloorPlanReservationRule
{
    public List<string> Check(FloorPlanReservation item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanReservationCodePrefixRule(string Prefix) : IFloorPlanReservationRule
{
    public List<string> Check(FloorPlanReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanReservationExpiresAtRangeRule(int Min, int Max) : IFloorPlanReservationRule
{
    public List<string> Check(FloorPlanReservation item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanReservationCodeLengthRule(int MaxLength) : IFloorPlanReservationRule
{
    public List<string> Check(FloorPlanReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan reservation.</summary>
public sealed record FloorPlanReservationValidator(List<IFloorPlanReservationRule> Rules)
{
    public List<string> Validate(FloorPlanReservation item)
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

    public void RequireValid(FloorPlanReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanReservation> items)
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

/// <summary>Functions of floor plan reservation rules.</summary>
public static class FloorPlanReservationRules
{
    public static FloorPlanReservationValidator DefaultFloorPlanReservationValidator()
    {
        return new FloorPlanReservationValidator(
            new List<IFloorPlanReservationRule>
            {
                new FloorPlanReservationContactEmailLengthRule(3600),
                new FloorPlanReservationDescriptionLengthRule(120),
                new FloorPlanReservationCodePrefixRule("ACM"),
                new FloorPlanReservationExpiresAtRangeRule(32, 3600),
                new FloorPlanReservationCodeLengthRule(16),
            });
    }
}
