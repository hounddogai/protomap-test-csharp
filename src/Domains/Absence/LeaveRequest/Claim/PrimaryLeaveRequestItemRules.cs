namespace Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>A rule a primary leave request item must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLeaveRequestItemRule
{
    List<string> Check(PrimaryLeaveRequestItem item);
}

public sealed record PrimaryLeaveRequestItemRequiredCodeRule(string Message) : IPrimaryLeaveRequestItemRule
{
    public List<string> Check(PrimaryLeaveRequestItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestItemDiscountRangeRule(double Min, double Max) : IPrimaryLeaveRequestItemRule
{
    public List<string> Check(PrimaryLeaveRequestItem item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestItemAttemptsRangeRule(int Min, int Max) : IPrimaryLeaveRequestItemRule
{
    public List<string> Check(PrimaryLeaveRequestItem item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary leave request item.</summary>
public sealed record PrimaryLeaveRequestItemValidator(List<IPrimaryLeaveRequestItemRule> Rules)
{
    public List<string> Validate(PrimaryLeaveRequestItem item)
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

    public void RequireValid(PrimaryLeaveRequestItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLeaveRequestItem> items)
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

/// <summary>Functions of primary leave request item rules.</summary>
public static class PrimaryLeaveRequestItemRules
{
    public static PrimaryLeaveRequestItemValidator DefaultPrimaryLeaveRequestItemValidator()
    {
        return new PrimaryLeaveRequestItemValidator(
            new List<IPrimaryLeaveRequestItemRule>
            {
                new PrimaryLeaveRequestItemRequiredCodeRule("primary leave request item needs a value"),
                new PrimaryLeaveRequestItemDiscountRangeRule(7.1, 3833.9),
                new PrimaryLeaveRequestItemAttemptsRangeRule(32, 86400),
            });
    }
}
