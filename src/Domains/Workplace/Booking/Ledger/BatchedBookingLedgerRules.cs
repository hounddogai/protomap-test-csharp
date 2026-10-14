namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched booking ledger must pass. Check returns its problems, or none.</summary>
public interface IBatchedBookingLedgerRule
{
    List<string> Check(BatchedBookingLedger item);
}

public sealed record BatchedBookingLedgerMinimumRangeRule(double Min, double Max) : IBatchedBookingLedgerRule
{
    public List<string> Check(BatchedBookingLedger item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedBookingLedgerAllowedCodeRule(List<string> Allowed) : IBatchedBookingLedgerRule
{
    public List<string> Check(BatchedBookingLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingLedgerLocationLengthRule(int MaxLength) : IBatchedBookingLedgerRule
{
    public List<string> Check(BatchedBookingLedger item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingLedgerSegmentLengthRule(int MaxLength) : IBatchedBookingLedgerRule
{
    public List<string> Check(BatchedBookingLedger item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingLedgerCodeLengthRule(int MaxLength) : IBatchedBookingLedgerRule
{
    public List<string> Check(BatchedBookingLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingLedgerLabelLengthRule(int MaxLength) : IBatchedBookingLedgerRule
{
    public List<string> Check(BatchedBookingLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched booking ledger.</summary>
public sealed record BatchedBookingLedgerValidator(List<IBatchedBookingLedgerRule> Rules)
{
    public List<string> Validate(BatchedBookingLedger item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(BatchedBookingLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBookingLedger> items)
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

/// <summary>Functions of batched booking ledger rules.</summary>
public static class BatchedBookingLedgerRules
{
    public static BatchedBookingLedgerValidator DefaultBatchedBookingLedgerValidator()
    {
        return new BatchedBookingLedgerValidator(
            new List<IBatchedBookingLedgerRule>
            {
                new BatchedBookingLedgerMinimumRangeRule(0.2, 723.5),
                new BatchedBookingLedgerAllowedCodeRule(new List<string> { "batched" }),
                new BatchedBookingLedgerLocationLengthRule(3600),
                new BatchedBookingLedgerSegmentLengthRule(16),
                new BatchedBookingLedgerCodeLengthRule(64),
                new BatchedBookingLedgerLabelLengthRule(120),
            });
    }
}
