namespace Acme.Workforce.Engagement.Recognition.Template;

/// <summary>A rule a rolling recognition reservation must pass. Check returns its problems, or none.</summary>
public interface IRollingRecognitionReservationRule
{
    List<string> Check(RollingRecognitionReservation item);
}

public sealed record RollingRecognitionReservationRequiredCodeRule(string Message) : IRollingRecognitionReservationRule
{
    public List<string> Check(RollingRecognitionReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionReservationSegmentLengthRule(int MaxLength) : IRollingRecognitionReservationRule
{
    public List<string> Check(RollingRecognitionReservation item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionReservationRequiredTeamRule(string Message) : IRollingRecognitionReservationRule
{
    public List<string> Check(RollingRecognitionReservation item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionReservationCostRangeRule(double Min, double Max) : IRollingRecognitionReservationRule
{
    public List<string> Check(RollingRecognitionReservation item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling recognition reservation.</summary>
public sealed record RollingRecognitionReservationValidator(List<IRollingRecognitionReservationRule> Rules)
{
    public List<string> Validate(RollingRecognitionReservation item)
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

    public void RequireValid(RollingRecognitionReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRecognitionReservation> items)
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

/// <summary>Functions of rolling recognition reservation rules.</summary>
public static class RollingRecognitionReservationRules
{
    public static RollingRecognitionReservationValidator DefaultRollingRecognitionReservationValidator()
    {
        return new RollingRecognitionReservationValidator(
            new List<IRollingRecognitionReservationRule>
            {
                new RollingRecognitionReservationRequiredCodeRule("rolling recognition reservation needs a value"),
                new RollingRecognitionReservationSegmentLengthRule(3600),
                new RollingRecognitionReservationRequiredTeamRule("rolling recognition reservation needs this field"),
                new RollingRecognitionReservationCostRangeRule(4.9, 3440.0),
            });
    }
}
