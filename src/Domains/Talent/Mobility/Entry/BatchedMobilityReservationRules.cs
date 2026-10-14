namespace Acme.Workforce.Talent.Mobility.Entry;

/// <summary>A rule a batched mobility reservation must pass. Check returns its problems, or none.</summary>
public interface IBatchedMobilityReservationRule
{
    List<string> Check(BatchedMobilityReservation item);
}

public sealed record BatchedMobilityReservationCodeLengthRule(int MaxLength) : IBatchedMobilityReservationRule
{
    public List<string> Check(BatchedMobilityReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityReservationLocaleLengthRule(int MaxLength) : IBatchedMobilityReservationRule
{
    public List<string> Check(BatchedMobilityReservation item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityReservationTitleLengthRule(int MaxLength) : IBatchedMobilityReservationRule
{
    public List<string> Check(BatchedMobilityReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMobilityReservationRequiredCodeRule(string Message) : IBatchedMobilityReservationRule
{
    public List<string> Check(BatchedMobilityReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched mobility reservation.</summary>
public sealed record BatchedMobilityReservationValidator(List<IBatchedMobilityReservationRule> Rules)
{
    public List<string> Validate(BatchedMobilityReservation item)
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

    public void RequireValid(BatchedMobilityReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedMobilityReservation> items)
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

/// <summary>Functions of batched mobility reservation rules.</summary>
public static class BatchedMobilityReservationRules
{
    public static BatchedMobilityReservationValidator DefaultBatchedMobilityReservationValidator()
    {
        return new BatchedMobilityReservationValidator(
            new List<IBatchedMobilityReservationRule>
            {
                new BatchedMobilityReservationCodeLengthRule(16),
                new BatchedMobilityReservationLocaleLengthRule(1000),
                new BatchedMobilityReservationTitleLengthRule(80),
                new BatchedMobilityReservationRequiredCodeRule("batched mobility reservation needs this field"),
            });
    }
}
