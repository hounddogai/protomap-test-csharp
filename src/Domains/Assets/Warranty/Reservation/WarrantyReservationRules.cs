namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty reservation must pass. Check returns its problems, or none.</summary>
public interface IWarrantyReservationRule
{
    List<string> Check(WarrantyReservation item);
}

public sealed record WarrantyReservationCodeLengthRule(int MaxLength) : IWarrantyReservationRule
{
    public List<string> Check(WarrantyReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReservationAllowedManagerIdRule(List<string> Allowed) : IWarrantyReservationRule
{
    public List<string> Check(WarrantyReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReservationRequiredBadgeRule(string Message) : IWarrantyReservationRule
{
    public List<string> Check(WarrantyReservation item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReservationThresholdRangeRule(double Min, double Max) : IWarrantyReservationRule
{
    public List<string> Check(WarrantyReservation item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReservationCodePrefixRule(string Prefix) : IWarrantyReservationRule
{
    public List<string> Check(WarrantyReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty reservation.</summary>
public sealed record WarrantyReservationValidator(List<IWarrantyReservationRule> Rules)
{
    public List<string> Validate(WarrantyReservation item)
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

    public void RequireValid(WarrantyReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyReservation> items)
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

/// <summary>Functions of warranty reservation rules.</summary>
public static class WarrantyReservationRules
{
    public static WarrantyReservationValidator DefaultWarrantyReservationValidator()
    {
        return new WarrantyReservationValidator(
            new List<IWarrantyReservationRule>
            {
                new WarrantyReservationCodeLengthRule(86400),
                new WarrantyReservationAllowedManagerIdRule(new List<string> { "scheduled", "regional", "fallback" }),
                new WarrantyReservationRequiredBadgeRule("warranty reservation needs an entry"),
                new WarrantyReservationThresholdRangeRule(8.3, 4012.4),
                new WarrantyReservationCodePrefixRule("OPS"),
            });
    }
}
