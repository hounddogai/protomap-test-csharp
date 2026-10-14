namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding policy is in its life cycle.</summary>
public enum OffboardingPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One offboarding policy, with what the planners need to act on it.</summary>
public sealed class OffboardingPolicy(string code, string title, double maximum, double ratio, int endsAt, string team, OffboardingPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public string Team { get; set; } = team;
    public OffboardingPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding policy in status idle.</summary>
    public static OffboardingPolicy NewOffboardingPolicy(string code, string title)
    {
        return new OffboardingPolicy(code, title, 0.0, 0.0, 0, "", OffboardingPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingPolicyStatus next)
    {
        switch (Status)
        {
            case OffboardingPolicyStatus.Idle:
                switch (next)
                {
                    case OffboardingPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingPolicyStatus.Warming:
                switch (next)
                {
                    case OffboardingPolicyStatus.Ready:
                    case OffboardingPolicyStatus.Draining:
                        return true;
                }
                return false;
            case OffboardingPolicyStatus.Ready:
                return next == OffboardingPolicyStatus.Draining;
            case OffboardingPolicyStatus.Draining:
                return next == OffboardingPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(OffboardingPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)EndsAt - Maximum, 2);
    }

    public OffboardingPolicy Copy()
    {
        return new OffboardingPolicy(Code, Title, Maximum, Ratio, EndsAt, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
