namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual refresh reservation must pass. Check returns its problems, or none.</summary>
public interface IManualRefreshReservationRule
{
    List<string> Check(ManualRefreshReservation item);
}

public sealed record ManualRefreshReservationExpiresAtRangeRule(int Min, int Max) : IManualRefreshReservationRule
{
    public List<string> Check(ManualRefreshReservation item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualRefreshReservationAllowedRegionRule(List<string> Allowed) : IManualRefreshReservationRule
{
    public List<string> Check(ManualRefreshReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ManualRefreshReservationPhoneLengthRule(int MaxLength) : IManualRefreshReservationRule
{
    public List<string> Check(ManualRefreshReservation item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualRefreshReservationSourceLengthRule(int MaxLength) : IManualRefreshReservationRule
{
    public List<string> Check(ManualRefreshReservation item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual refresh reservation.</summary>
public sealed record ManualRefreshReservationValidator(List<IManualRefreshReservationRule> Rules)
{
    public List<string> Validate(ManualRefreshReservation item)
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

    public void RequireValid(ManualRefreshReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualRefreshReservation> items)
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

/// <summary>Functions of manual refresh reservation rules.</summary>
public static class ManualRefreshReservationRules
{
    public static ManualRefreshReservationValidator DefaultManualRefreshReservationValidator()
    {
        return new ManualRefreshReservationValidator(
            new List<IManualRefreshReservationRule>
            {
                new ManualRefreshReservationExpiresAtRangeRule(32, 64),
                new ManualRefreshReservationAllowedRegionRule(new List<string> { "strict", "batched" }),
                new ManualRefreshReservationPhoneLengthRule(32),
                new ManualRefreshReservationSourceLengthRule(32),
            });
    }
}
