namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped pay run reservation must pass. Check returns its problems, or none.</summary>
public interface ICappedPayRunReservationRule
{
    List<string> Check(CappedPayRunReservation item);
}

public sealed record CappedPayRunReservationTitleLengthRule(int MaxLength) : ICappedPayRunReservationRule
{
    public List<string> Check(CappedPayRunReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunReservationAllowedSummaryRule(List<string> Allowed) : ICappedPayRunReservationRule
{
    public List<string> Check(CappedPayRunReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayRunReservationRequiredTitleRule(string Message) : ICappedPayRunReservationRule
{
    public List<string> Check(CappedPayRunReservation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pay run reservation.</summary>
public sealed record CappedPayRunReservationValidator(List<ICappedPayRunReservationRule> Rules)
{
    public List<string> Validate(CappedPayRunReservation item)
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

    public void RequireValid(CappedPayRunReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayRunReservation> items)
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

/// <summary>Functions of capped pay run reservation rules.</summary>
public static class CappedPayRunReservationRules
{
    public static CappedPayRunReservationValidator DefaultCappedPayRunReservationValidator()
    {
        return new CappedPayRunReservationValidator(
            new List<ICappedPayRunReservationRule>
            {
                new CappedPayRunReservationTitleLengthRule(3600),
                new CappedPayRunReservationAllowedSummaryRule(new List<string> { "tiered", "global" }),
                new CappedPayRunReservationRequiredTitleRule("capped pay run reservation needs this field"),
            });
    }
}
