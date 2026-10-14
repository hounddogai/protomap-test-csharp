namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination policy is in its life cycle.</summary>
public enum NominationPolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A nomination policy. Its code is unique within a tenant.</summary>
public sealed class NominationPolicy(string code, string label, string jobCode, double minimum, int capacity, int startsAt, string grade, NominationPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public NominationPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a nomination policy in status requested.</summary>
    public static NominationPolicy NewNominationPolicy(string code, string label)
    {
        return new NominationPolicy(code, label, "", 0.0, 0, 0, "", NominationPolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationPolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationPolicyStatus next)
    {
        switch (Status)
        {
            case NominationPolicyStatus.Requested:
                return next == NominationPolicyStatus.Granted;
            case NominationPolicyStatus.Granted:
                switch (next)
                {
                    case NominationPolicyStatus.Revoked:
                        return true;
                }
                return false;
            case NominationPolicyStatus.Revoked:
                return next == NominationPolicyStatus.Expired;
        }
        return false;
    }

    public void MoveTo(NominationPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum - (double)Capacity, 2);
    }

    public NominationPolicy Copy()
    {
        return new NominationPolicy(Code, Label, JobCode, Minimum, Capacity, StartsAt, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
