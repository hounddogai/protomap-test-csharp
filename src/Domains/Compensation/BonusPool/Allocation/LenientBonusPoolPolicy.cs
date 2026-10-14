namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient bonus pool policy is in its life cycle.</summary>
public enum LenientBonusPoolPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient bonus pool policy, with what the planners need to act on it.</summary>
public sealed class LenientBonusPoolPolicy(string code, string name, int version, double hours, int limit, double fte, int windowDays, string checksum, string location, string externalRef, string grade, LenientBonusPoolPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public int Limit { get; set; } = limit;
    public double Fte { get; set; } = fte;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public string Grade { get; set; } = grade;
    public LenientBonusPoolPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a lenient bonus pool policy in status open.</summary>
    public static LenientBonusPoolPolicy NewLenientBonusPoolPolicy(string code, string name)
    {
        return new LenientBonusPoolPolicy(
            code,
            name,
            0,
            0.0,
            0,
            0.0,
            0,
            "",
            "",
            "",
            "",
            LenientBonusPoolPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBonusPoolPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBonusPoolPolicyStatus next)
    {
        switch (Status)
        {
            case LenientBonusPoolPolicyStatus.Open:
                return next == LenientBonusPoolPolicyStatus.InReview;
            case LenientBonusPoolPolicyStatus.InReview:
                return next == LenientBonusPoolPolicyStatus.Resolved;
            case LenientBonusPoolPolicyStatus.Resolved:
                switch (next)
                {
                    case LenientBonusPoolPolicyStatus.Reopened:
                    case LenientBonusPoolPolicyStatus.Archived:
                        return true;
                }
                return false;
            case LenientBonusPoolPolicyStatus.Reopened:
                switch (next)
                {
                    case LenientBonusPoolPolicyStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBonusPoolPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient bonus pool policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"lenient bonus pool policy"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)WindowDays - Hours, 2);
    }

    public LenientBonusPoolPolicy Copy()
    {
        return new LenientBonusPoolPolicy(
            Code,
            Name,
            Version,
            Hours,
            Limit,
            Fte,
            WindowDays,
            Checksum,
            Location,
            ExternalRef,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
