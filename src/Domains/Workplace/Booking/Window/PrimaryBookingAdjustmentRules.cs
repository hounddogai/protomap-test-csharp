namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary booking adjustment must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBookingAdjustmentRule
{
    List<string> Check(PrimaryBookingAdjustment item);
}

public sealed record PrimaryBookingAdjustmentRetriesRangeRule(int Min, int Max) : IPrimaryBookingAdjustmentRule
{
    public List<string> Check(PrimaryBookingAdjustment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingAdjustmentRequiredDepartmentRule(string Message) : IPrimaryBookingAdjustmentRule
{
    public List<string> Check(PrimaryBookingAdjustment item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingAdjustmentCodePrefixRule(string Prefix) : IPrimaryBookingAdjustmentRule
{
    public List<string> Check(PrimaryBookingAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingAdjustmentWeightRangeRule(double Min, double Max) : IPrimaryBookingAdjustmentRule
{
    public List<string> Check(PrimaryBookingAdjustment item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingAdjustmentAllowedDescriptionRule(List<string> Allowed) : IPrimaryBookingAdjustmentRule
{
    public List<string> Check(PrimaryBookingAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary booking adjustment.</summary>
public sealed record PrimaryBookingAdjustmentValidator(List<IPrimaryBookingAdjustmentRule> Rules)
{
    public List<string> Validate(PrimaryBookingAdjustment item)
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

    public void RequireValid(PrimaryBookingAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBookingAdjustment> items)
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

/// <summary>Functions of primary booking adjustment rules.</summary>
public static class PrimaryBookingAdjustmentRules
{
    public static PrimaryBookingAdjustmentValidator DefaultPrimaryBookingAdjustmentValidator()
    {
        return new PrimaryBookingAdjustmentValidator(
            new List<IPrimaryBookingAdjustmentRule>
            {
                new PrimaryBookingAdjustmentRetriesRangeRule(16, 86400),
                new PrimaryBookingAdjustmentRequiredDepartmentRule("primary booking adjustment needs this field"),
                new PrimaryBookingAdjustmentCodePrefixRule("Q"),
                new PrimaryBookingAdjustmentWeightRangeRule(7.0, 1071.9),
                new PrimaryBookingAdjustmentAllowedDescriptionRule(new List<string> { "batched", "capped" }),
            });
    }
}
