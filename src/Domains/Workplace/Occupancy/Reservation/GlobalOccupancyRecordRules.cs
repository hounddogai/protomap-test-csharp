namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global occupancy record must pass. Check returns its problems, or none.</summary>
public interface IGlobalOccupancyRecordRule
{
    List<string> Check(GlobalOccupancyRecord item);
}

public sealed record GlobalOccupancyRecordAllowedExternalRefRule(List<string> Allowed) : IGlobalOccupancyRecordRule
{
    public List<string> Check(GlobalOccupancyRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record GlobalOccupancyRecordRequiredNotesRule(string Message) : IGlobalOccupancyRecordRule
{
    public List<string> Check(GlobalOccupancyRecord item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalOccupancyRecordCodePrefixRule(string Prefix) : IGlobalOccupancyRecordRule
{
    public List<string> Check(GlobalOccupancyRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global occupancy record.</summary>
public sealed record GlobalOccupancyRecordValidator(List<IGlobalOccupancyRecordRule> Rules)
{
    public List<string> Validate(GlobalOccupancyRecord item)
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

    public void RequireValid(GlobalOccupancyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalOccupancyRecord> items)
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

/// <summary>Functions of global occupancy record rules.</summary>
public static class GlobalOccupancyRecordRules
{
    public static GlobalOccupancyRecordValidator DefaultGlobalOccupancyRecordValidator()
    {
        return new GlobalOccupancyRecordValidator(
            new List<IGlobalOccupancyRecordRule>
            {
                new GlobalOccupancyRecordAllowedExternalRefRule(new List<string>()),
                new GlobalOccupancyRecordRequiredNotesRule("global occupancy record needs an entry"),
                new GlobalOccupancyRecordCodePrefixRule("OPS"),
            });
    }
}
