namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient vendor reservation must pass. Check returns its problems, or none.</summary>
public interface ILenientVendorReservationRule
{
    List<string> Check(LenientVendorReservation item);
}

public sealed record LenientVendorReservationLocaleLengthRule(int MaxLength) : ILenientVendorReservationRule
{
    public List<string> Check(LenientVendorReservation item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorReservationBatchSizeRangeRule(int Min, int Max) : ILenientVendorReservationRule
{
    public List<string> Check(LenientVendorReservation item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorReservationCurrencyLengthRule(int MaxLength) : ILenientVendorReservationRule
{
    public List<string> Check(LenientVendorReservation item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorReservationCodeLengthRule(int MaxLength) : ILenientVendorReservationRule
{
    public List<string> Check(LenientVendorReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVendorReservationAllowedNameRule(List<string> Allowed) : ILenientVendorReservationRule
{
    public List<string> Check(LenientVendorReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient vendor reservation.</summary>
public sealed record LenientVendorReservationValidator(List<ILenientVendorReservationRule> Rules)
{
    public List<string> Validate(LenientVendorReservation item)
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

    public void RequireValid(LenientVendorReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVendorReservation> items)
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

/// <summary>Functions of lenient vendor reservation rules.</summary>
public static class LenientVendorReservationRules
{
    public static LenientVendorReservationValidator DefaultLenientVendorReservationValidator()
    {
        return new LenientVendorReservationValidator(
            new List<ILenientVendorReservationRule>
            {
                new LenientVendorReservationLocaleLengthRule(64),
                new LenientVendorReservationBatchSizeRangeRule(64, 255),
                new LenientVendorReservationCurrencyLengthRule(64),
                new LenientVendorReservationCodeLengthRule(32),
                new LenientVendorReservationAllowedNameRule(new List<string> { "scheduled" }),
            });
    }
}
