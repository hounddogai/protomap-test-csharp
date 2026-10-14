namespace Acme.Workforce.Workplace.Booking.Quota;

/// <summary>A rule a adaptive booking profile must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBookingProfileRule
{
    List<string> Check(AdaptiveBookingProfile item);
}

public sealed record AdaptiveBookingProfileJobCodeLengthRule(int MaxLength) : IAdaptiveBookingProfileRule
{
    public List<string> Check(AdaptiveBookingProfile item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBookingProfileCodeLengthRule(int MaxLength) : IAdaptiveBookingProfileRule
{
    public List<string> Check(AdaptiveBookingProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBookingProfileLabelLengthRule(int MaxLength) : IAdaptiveBookingProfileRule
{
    public List<string> Check(AdaptiveBookingProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBookingProfileActiveGuardRule(int Limit) : IAdaptiveBookingProfileRule
{
    public List<string> Check(AdaptiveBookingProfile item)
    {
        if (item.Active && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBookingProfileRequiredJobCodeRule(string Message) : IAdaptiveBookingProfileRule
{
    public List<string> Check(AdaptiveBookingProfile item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive booking profile.</summary>
public sealed record AdaptiveBookingProfileValidator(List<IAdaptiveBookingProfileRule> Rules)
{
    public List<string> Validate(AdaptiveBookingProfile item)
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

    public void RequireValid(AdaptiveBookingProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBookingProfile> items)
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

/// <summary>Functions of adaptive booking profile rules.</summary>
public static class AdaptiveBookingProfileRules
{
    public static AdaptiveBookingProfileValidator DefaultAdaptiveBookingProfileValidator()
    {
        return new AdaptiveBookingProfileValidator(
            new List<IAdaptiveBookingProfileRule>
            {
                new AdaptiveBookingProfileJobCodeLengthRule(1000),
                new AdaptiveBookingProfileCodeLengthRule(1000),
                new AdaptiveBookingProfileLabelLengthRule(3600),
                new AdaptiveBookingProfileActiveGuardRule(3600),
                new AdaptiveBookingProfileRequiredJobCodeRule("adaptive booking profile needs this field"),
            });
    }
}
