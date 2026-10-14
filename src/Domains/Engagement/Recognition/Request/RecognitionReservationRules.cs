namespace Acme.Workforce.Engagement.Recognition.Request;

/// <summary>A rule a recognition reservation must pass. Check returns its problems, or none.</summary>
public interface IRecognitionReservationRule
{
    List<string> Check(RecognitionReservation item);
}

public sealed record RecognitionReservationCodeLengthRule(int MaxLength) : IRecognitionReservationRule
{
    public List<string> Check(RecognitionReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReservationGraceDaysRangeRule(int Min, int Max) : IRecognitionReservationRule
{
    public List<string> Check(RecognitionReservation item)
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

public sealed record RecognitionReservationRequiredTimezoneRule(string Message) : IRecognitionReservationRule
{
    public List<string> Check(RecognitionReservation item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionReservationRequiredOwnerIdRule(string Message) : IRecognitionReservationRule
{
    public List<string> Check(RecognitionReservation item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition reservation.</summary>
public sealed record RecognitionReservationValidator(List<IRecognitionReservationRule> Rules)
{
    public List<string> Validate(RecognitionReservation item)
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

    public void RequireValid(RecognitionReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionReservation> items)
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

/// <summary>Functions of recognition reservation rules.</summary>
public static class RecognitionReservationRules
{
    public static RecognitionReservationValidator DefaultRecognitionReservationValidator()
    {
        return new RecognitionReservationValidator(
            new List<IRecognitionReservationRule>
            {
                new RecognitionReservationCodeLengthRule(1000),
                new RecognitionReservationGraceDaysRangeRule(16, 255),
                new RecognitionReservationRequiredTimezoneRule("recognition reservation needs an entry"),
                new RecognitionReservationRequiredOwnerIdRule("recognition reservation needs an entry"),
            });
    }
}
