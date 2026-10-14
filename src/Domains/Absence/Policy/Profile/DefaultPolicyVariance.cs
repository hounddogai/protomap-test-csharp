namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy variance is in its life cycle.</summary>
public enum DefaultPolicyVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default policy variance, with what the planners need to act on it.</summary>
public sealed class DefaultPolicyVariance(string code, string label, int startsAt, double cost, string skill, double discount, double rating, string segment, string location, string externalRef, int priority, DefaultPolicyVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public int Priority { get; set; } = priority;
    public DefaultPolicyVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a default policy variance in status draft.</summary>
    public static DefaultPolicyVariance NewDefaultPolicyVariance(string code, string label)
    {
        return new DefaultPolicyVariance(
            code,
            label,
            0,
            0.0,
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            0,
            DefaultPolicyVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyVarianceStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyVarianceStatus.Draft:
                switch (next)
                {
                    case DefaultPolicyVarianceStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultPolicyVarianceStatus.Pending:
                switch (next)
                {
                    case DefaultPolicyVarianceStatus.Active:
                    case DefaultPolicyVarianceStatus.Closed:
                        return true;
                }
                return false;
            case DefaultPolicyVarianceStatus.Active:
                return next == DefaultPolicyVarianceStatus.Suspended;
            case DefaultPolicyVarianceStatus.Suspended:
                switch (next)
                {
                    case DefaultPolicyVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"default policy variance"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)StartsAt * Discount, 2);
    }

    public DefaultPolicyVariance Copy()
    {
        return new DefaultPolicyVariance(
            Code,
            Label,
            StartsAt,
            Cost,
            Skill,
            Discount,
            Rating,
            Segment,
            Location,
            ExternalRef,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
