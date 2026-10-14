namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict career path digest is in its life cycle.</summary>
public enum StrictCareerPathDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a strict career path digest and its current status.</summary>
public sealed class StrictCareerPathDigest(string code, string name, double budget, string checksum, string country, StrictCareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Checksum { get; set; } = checksum;
    public string Country { get; set; } = country;
    public StrictCareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a strict career path digest in status proposed.</summary>
    public static StrictCareerPathDigest NewStrictCareerPathDigest(string code, string name)
    {
        return new StrictCareerPathDigest(code, name, 0.0, "", "", StrictCareerPathDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictCareerPathDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictCareerPathDigestStatus next)
    {
        switch (Status)
        {
            case StrictCareerPathDigestStatus.Proposed:
                return next == StrictCareerPathDigestStatus.Rejected;
            case StrictCareerPathDigestStatus.Approved:
                switch (next)
                {
                    case StrictCareerPathDigestStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictCareerPathDigestStatus.Rejected:
                return next == StrictCareerPathDigestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictCareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public StrictCareerPathDigest Copy()
    {
        return new StrictCareerPathDigest(Code, Name, Budget, Checksum, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
