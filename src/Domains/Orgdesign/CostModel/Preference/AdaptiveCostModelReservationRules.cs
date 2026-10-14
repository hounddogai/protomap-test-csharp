namespace Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>A rule a adaptive cost model reservation must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCostModelReservationRule
{
    List<string> Check(AdaptiveCostModelReservation item);
}

public sealed record AdaptiveCostModelReservationRequiredLocationRule(string Message) : IAdaptiveCostModelReservationRule
{
    public List<string> Check(AdaptiveCostModelReservation item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelReservationSummaryLengthRule(int MaxLength) : IAdaptiveCostModelReservationRule
{
    public List<string> Check(AdaptiveCostModelReservation item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelReservationCodeLengthRule(int MaxLength) : IAdaptiveCostModelReservationRule
{
    public List<string> Check(AdaptiveCostModelReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive cost model reservation.</summary>
public sealed record AdaptiveCostModelReservationValidator(List<IAdaptiveCostModelReservationRule> Rules)
{
    public List<string> Validate(AdaptiveCostModelReservation item)
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

    public void RequireValid(AdaptiveCostModelReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCostModelReservation> items)
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

/// <summary>Functions of adaptive cost model reservation rules.</summary>
public static class AdaptiveCostModelReservationRules
{
    public static AdaptiveCostModelReservationValidator DefaultAdaptiveCostModelReservationValidator()
    {
        return new AdaptiveCostModelReservationValidator(
            new List<IAdaptiveCostModelReservationRule>
            {
                new AdaptiveCostModelReservationRequiredLocationRule(
                    "adaptive cost model reservation needs this field"),
                new AdaptiveCostModelReservationSummaryLengthRule(3600),
                new AdaptiveCostModelReservationCodeLengthRule(64),
            });
    }
}
