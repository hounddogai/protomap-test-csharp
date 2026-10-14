namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy rollup is in its life cycle.</summary>
public enum LenientPolicyRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient policy rollup as the policy team keeps it.</summary>
public sealed class LenientPolicyRollup(string code, string name, string contactEmail, string label, double minimum, string department, int revision, string reviewer, string locale, LenientPolicyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Department { get; set; } = department;
    public int Revision { get; set; } = revision;
    public string Reviewer { get; set; } = reviewer;
    public string Locale { get; set; } = locale;
    public LenientPolicyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy rollup in status requested.</summary>
    public static LenientPolicyRollup NewLenientPolicyRollup(string code, string name)
    {
        return new LenientPolicyRollup(code, name, "", "", 0.0, "", 0, "", "", LenientPolicyRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyRollupStatus next)
    {
        switch (Status)
        {
            case LenientPolicyRollupStatus.Requested:
                switch (next)
                {
                    case LenientPolicyRollupStatus.Granted:
                    case LenientPolicyRollupStatus.Expired:
                        return true;
                }
                return false;
            case LenientPolicyRollupStatus.Granted:
                return next == LenientPolicyRollupStatus.Revoked || next == LenientPolicyRollupStatus.Expired;
            case LenientPolicyRollupStatus.Revoked:
                switch (next)
                {
                    case LenientPolicyRollupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPolicyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round((double)Revision + Minimum, 2);
    }

    public LenientPolicyRollup Copy()
    {
        return new LenientPolicyRollup(
            Code,
            Name,
            ContactEmail,
            Label,
            Minimum,
            Department,
            Revision,
            Reviewer,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
