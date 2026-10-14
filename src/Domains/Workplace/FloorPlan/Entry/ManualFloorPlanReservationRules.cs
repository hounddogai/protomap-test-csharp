namespace Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>A rule a manual floor plan reservation must pass. Check returns its problems, or none.</summary>
public interface IManualFloorPlanReservationRule
{
    List<string> Check(ManualFloorPlanReservation item);
}

public sealed record ManualFloorPlanReservationRequiredCodeRule(string Message) : IManualFloorPlanReservationRule
{
    public List<string> Check(ManualFloorPlanReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanReservationMarginRangeRule(double Min, double Max) : IManualFloorPlanReservationRule
{
    public List<string> Check(ManualFloorPlanReservation item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualFloorPlanReservationGraceDaysRangeRule(int Min, int Max) : IManualFloorPlanReservationRule
{
    public List<string> Check(ManualFloorPlanReservation item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual floor plan reservation.</summary>
public sealed record ManualFloorPlanReservationValidator(List<IManualFloorPlanReservationRule> Rules)
{
    public List<string> Validate(ManualFloorPlanReservation item)
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

    public void RequireValid(ManualFloorPlanReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualFloorPlanReservation> items)
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

/// <summary>Functions of manual floor plan reservation rules.</summary>
public static class ManualFloorPlanReservationRules
{
    public static ManualFloorPlanReservationValidator DefaultManualFloorPlanReservationValidator()
    {
        return new ManualFloorPlanReservationValidator(
            new List<IManualFloorPlanReservationRule>
            {
                new ManualFloorPlanReservationRequiredCodeRule("manual floor plan reservation needs this field"),
                new ManualFloorPlanReservationMarginRangeRule(3.9, 987.4),
                new ManualFloorPlanReservationGraceDaysRangeRule(255, 3600),
            });
    }
}
