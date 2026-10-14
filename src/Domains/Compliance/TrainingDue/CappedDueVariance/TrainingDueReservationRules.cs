namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a training due reservation must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueReservationRule
{
    List<string> Check(TrainingDueReservation item);
}

public sealed record TrainingDueReservationCodePrefixRule(string Prefix) : ITrainingDueReservationRule
{
    public List<string> Check(TrainingDueReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueReservationCodeLengthRule(int MaxLength) : ITrainingDueReservationRule
{
    public List<string> Check(TrainingDueReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueReservationRequiredEmployeeIdRule(string Message) : ITrainingDueReservationRule
{
    public List<string> Check(TrainingDueReservation item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueReservationAllowedCodeRule(List<string> Allowed) : ITrainingDueReservationRule
{
    public List<string> Check(TrainingDueReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due reservation.</summary>
public sealed record TrainingDueReservationValidator(List<ITrainingDueReservationRule> Rules)
{
    public List<string> Validate(TrainingDueReservation item)
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

    public void RequireValid(TrainingDueReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueReservation> items)
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

/// <summary>Functions of training due reservation rules.</summary>
public static class TrainingDueReservationRules
{
    public static TrainingDueReservationValidator DefaultTrainingDueReservationValidator()
    {
        return new TrainingDueReservationValidator(
            new List<ITrainingDueReservationRule>
            {
                new TrainingDueReservationCodePrefixRule("X"),
                new TrainingDueReservationCodeLengthRule(255),
                new TrainingDueReservationRequiredEmployeeIdRule("training due reservation needs this field"),
                new TrainingDueReservationAllowedCodeRule(new List<string> { "lenient" }),
            });
    }
}
