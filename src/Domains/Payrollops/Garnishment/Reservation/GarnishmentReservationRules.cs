namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>A rule a garnishment reservation must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentReservationRule
{
    List<string> Check(GarnishmentReservation item);
}

public sealed record GarnishmentReservationTagsLimitRule(int MaxCount, List<string> Banned) : IGarnishmentReservationRule
{
    public List<string> Check(GarnishmentReservation item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record GarnishmentReservationCodeLengthRule(int MaxLength) : IGarnishmentReservationRule
{
    public List<string> Check(GarnishmentReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentReservationCreatedAtRangeRule(int Min, int Max) : IGarnishmentReservationRule
{
    public List<string> Check(GarnishmentReservation item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a garnishment reservation.</summary>
public sealed record GarnishmentReservationValidator(List<IGarnishmentReservationRule> Rules)
{
    public List<string> Validate(GarnishmentReservation item)
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

    public void RequireValid(GarnishmentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentReservation> items)
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

/// <summary>Functions of garnishment reservation rules.</summary>
public static class GarnishmentReservationRules
{
    public static GarnishmentReservationValidator DefaultGarnishmentReservationValidator()
    {
        return new GarnishmentReservationValidator(
            new List<IGarnishmentReservationRule>
            {
                new GarnishmentReservationTagsLimitRule(120, new List<string> { "rolling", "batched", "pending" }),
                new GarnishmentReservationCodeLengthRule(255),
                new GarnishmentReservationCreatedAtRangeRule(120, 255),
            });
    }
}
