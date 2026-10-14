namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>A rule a pending license seat slot must pass. Check returns its problems, or none.</summary>
public interface IPendingLicenseSeatSlotRule
{
    List<string> Check(PendingLicenseSeatSlot item);
}

public sealed record PendingLicenseSeatSlotActiveGuardRule(int Limit) : IPendingLicenseSeatSlotRule
{
    public List<string> Check(PendingLicenseSeatSlot item)
    {
        if (item.Active && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatSlotRequiredLabelRule(string Message) : IPendingLicenseSeatSlotRule
{
    public List<string> Check(PendingLicenseSeatSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingLicenseSeatSlotRequiredCategoryRule(string Message) : IPendingLicenseSeatSlotRule
{
    public List<string> Check(PendingLicenseSeatSlot item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending license seat slot.</summary>
public sealed record PendingLicenseSeatSlotValidator(List<IPendingLicenseSeatSlotRule> Rules)
{
    public List<string> Validate(PendingLicenseSeatSlot item)
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

    public void RequireValid(PendingLicenseSeatSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingLicenseSeatSlot> items)
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

/// <summary>Functions of pending license seat slot rules.</summary>
public static class PendingLicenseSeatSlotRules
{
    public static PendingLicenseSeatSlotValidator DefaultPendingLicenseSeatSlotValidator()
    {
        return new PendingLicenseSeatSlotValidator(
            new List<IPendingLicenseSeatSlotRule>
            {
                new PendingLicenseSeatSlotActiveGuardRule(86400),
                new PendingLicenseSeatSlotRequiredLabelRule("pending license seat slot needs an entry"),
                new PendingLicenseSeatSlotRequiredCategoryRule("pending license seat slot needs an entry"),
            });
    }
}
