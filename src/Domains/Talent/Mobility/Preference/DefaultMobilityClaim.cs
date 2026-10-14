namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default mobility claim is in its life cycle.</summary>
public enum DefaultMobilityClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One default mobility claim, with what the planners need to act on it.</summary>
public sealed class DefaultMobilityClaim(string code, string name, string team, string currency, double maximum, int quantity, bool pinned, DefaultMobilityClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public DefaultMobilityClaimStatus Status { get; set; } = status;

    /// <summary>Starts a default mobility claim in status idle.</summary>
    public static DefaultMobilityClaim NewDefaultMobilityClaim(string code, string name)
    {
        return new DefaultMobilityClaim(code, name, "", "", 0.0, 0, false, DefaultMobilityClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultMobilityClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultMobilityClaimStatus next)
    {
        switch (Status)
        {
            case DefaultMobilityClaimStatus.Idle:
                switch (next)
                {
                    case DefaultMobilityClaimStatus.Ready:
                    case DefaultMobilityClaimStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultMobilityClaimStatus.Warming:
                return next == DefaultMobilityClaimStatus.Draining || next == DefaultMobilityClaimStatus.Stopped;
            case DefaultMobilityClaimStatus.Ready:
                switch (next)
                {
                    case DefaultMobilityClaimStatus.Stopped:
                        return true;
                }
                return false;
            case DefaultMobilityClaimStatus.Draining:
                switch (next)
                {
                    case DefaultMobilityClaimStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultMobilityClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default mobility claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default mobility claim"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Maximum * (double)Quantity, 2);
    }

    public DefaultMobilityClaim Copy()
    {
        return new DefaultMobilityClaim(Code, Name, Team, Currency, Maximum, Quantity, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
