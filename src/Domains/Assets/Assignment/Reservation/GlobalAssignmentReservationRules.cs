namespace Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>A rule a global assignment reservation must pass. Check returns its problems, or none.</summary>
public interface IGlobalAssignmentReservationRule
{
    List<string> Check(GlobalAssignmentReservation item);
}

public sealed record GlobalAssignmentReservationRequiredTitleRule(string Message) : IGlobalAssignmentReservationRule
{
    public List<string> Check(GlobalAssignmentReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssignmentReservationCodeLengthRule(int MaxLength) : IGlobalAssignmentReservationRule
{
    public List<string> Check(GlobalAssignmentReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssignmentReservationDepartmentLengthRule(int MaxLength) : IGlobalAssignmentReservationRule
{
    public List<string> Check(GlobalAssignmentReservation item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssignmentReservationArchivedGuardRule(int Limit) : IGlobalAssignmentReservationRule
{
    public List<string> Check(GlobalAssignmentReservation item)
    {
        if (item.Archived && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssignmentReservationEndsAtRangeRule(int Min, int Max) : IGlobalAssignmentReservationRule
{
    public List<string> Check(GlobalAssignmentReservation item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global assignment reservation.</summary>
public sealed record GlobalAssignmentReservationValidator(List<IGlobalAssignmentReservationRule> Rules)
{
    public List<string> Validate(GlobalAssignmentReservation item)
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

    public void RequireValid(GlobalAssignmentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAssignmentReservation> items)
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

/// <summary>Functions of global assignment reservation rules.</summary>
public static class GlobalAssignmentReservationRules
{
    public static GlobalAssignmentReservationValidator DefaultGlobalAssignmentReservationValidator()
    {
        return new GlobalAssignmentReservationValidator(
            new List<IGlobalAssignmentReservationRule>
            {
                new GlobalAssignmentReservationRequiredTitleRule("global assignment reservation needs a value"),
                new GlobalAssignmentReservationCodeLengthRule(255),
                new GlobalAssignmentReservationDepartmentLengthRule(64),
                new GlobalAssignmentReservationArchivedGuardRule(64),
                new GlobalAssignmentReservationEndsAtRangeRule(16, 64),
            });
    }
}
