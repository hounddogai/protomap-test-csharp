namespace Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>A rule a batched booking audit must pass. Check returns its problems, or none.</summary>
public interface IBatchedBookingAuditRule
{
    List<string> Check(BatchedBookingAudit item);
}

public sealed record BatchedBookingAuditPinnedGuardRule(int Limit) : IBatchedBookingAuditRule
{
    public List<string> Check(BatchedBookingAudit item)
    {
        if (item.Pinned && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingAuditDescriptionLengthRule(int MaxLength) : IBatchedBookingAuditRule
{
    public List<string> Check(BatchedBookingAudit item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBookingAuditRequiredCodeRule(string Message) : IBatchedBookingAuditRule
{
    public List<string> Check(BatchedBookingAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched booking audit.</summary>
public sealed record BatchedBookingAuditValidator(List<IBatchedBookingAuditRule> Rules)
{
    public List<string> Validate(BatchedBookingAudit item)
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

    public void RequireValid(BatchedBookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBookingAudit> items)
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

/// <summary>Functions of batched booking audit rules.</summary>
public static class BatchedBookingAuditRules
{
    public static BatchedBookingAuditValidator DefaultBatchedBookingAuditValidator()
    {
        return new BatchedBookingAuditValidator(
            new List<IBatchedBookingAuditRule>
            {
                new BatchedBookingAuditPinnedGuardRule(32),
                new BatchedBookingAuditDescriptionLengthRule(255),
                new BatchedBookingAuditRequiredCodeRule("batched booking audit needs a value"),
            });
    }
}
