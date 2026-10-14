namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay run reservation must pass. Check returns its problems, or none.</summary>
public interface IPayRunReservationRule
{
    List<string> Check(PayRunReservation item);
}

public sealed record PayRunReservationAllowedTitleRule(List<string> Allowed) : IPayRunReservationRule
{
    public List<string> Check(PayRunReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PayRunReservationSummaryLengthRule(int MaxLength) : IPayRunReservationRule
{
    public List<string> Check(PayRunReservation item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunReservationRequiredCodeRule(string Message) : IPayRunReservationRule
{
    public List<string> Check(PayRunReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunReservationRequiredRegionRule(string Message) : IPayRunReservationRule
{
    public List<string> Check(PayRunReservation item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunReservationPinnedGuardRule(double Limit) : IPayRunReservationRule
{
    public List<string> Check(PayRunReservation item)
    {
        if (item.Pinned && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run reservation.</summary>
public sealed record PayRunReservationValidator(List<IPayRunReservationRule> Rules)
{
    public List<string> Validate(PayRunReservation item)
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

    public void RequireValid(PayRunReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunReservation> items)
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

/// <summary>Functions of pay run reservation rules.</summary>
public static class PayRunReservationRules
{
    public static PayRunReservationValidator DefaultPayRunReservationValidator()
    {
        return new PayRunReservationValidator(
            new List<IPayRunReservationRule>
            {
                new PayRunReservationAllowedTitleRule(new List<string> { "capped", "default", "primary" }),
                new PayRunReservationSummaryLengthRule(1000),
                new PayRunReservationRequiredCodeRule("pay run reservation needs an entry"),
                new PayRunReservationRequiredRegionRule("pay run reservation needs a value"),
                new PayRunReservationPinnedGuardRule(0.6),
            });
    }
}
