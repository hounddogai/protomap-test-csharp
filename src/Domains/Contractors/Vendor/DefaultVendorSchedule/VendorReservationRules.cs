namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>A rule a vendor reservation must pass. Check returns its problems, or none.</summary>
public interface IVendorReservationRule
{
    List<string> Check(VendorReservation item);
}

public sealed record VendorReservationRegionLengthRule(int MaxLength) : IVendorReservationRule
{
    public List<string> Check(VendorReservation item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VendorReservationRequiredRegionRule(string Message) : IVendorReservationRule
{
    public List<string> Check(VendorReservation item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VendorReservationTitleLengthRule(int MaxLength) : IVendorReservationRule
{
    public List<string> Check(VendorReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a vendor reservation.</summary>
public sealed record VendorReservationValidator(List<IVendorReservationRule> Rules)
{
    public List<string> Validate(VendorReservation item)
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

    public void RequireValid(VendorReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VendorReservation> items)
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

/// <summary>Functions of vendor reservation rules.</summary>
public static class VendorReservationRules
{
    public static VendorReservationValidator DefaultVendorReservationValidator()
    {
        return new VendorReservationValidator(
            new List<IVendorReservationRule>
            {
                new VendorReservationRegionLengthRule(64),
                new VendorReservationRequiredRegionRule("vendor reservation needs a value"),
                new VendorReservationTitleLengthRule(16),
            });
    }
}
