namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination bundle is in its life cycle.</summary>
public enum NominationBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A nomination bundle. Its code is unique within a tenant.</summary>
public sealed class NominationBundle(string code, string name, double rate, string checksum, string badge, double margin, int durationMinutes, List<string> tags, string grade, int expiresAt, string department, NominationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Checksum { get; set; } = checksum;
    public string Badge { get; set; } = badge;
    public double Margin { get; set; } = margin;
    public int DurationMinutes { get; set; } = durationMinutes;
    public List<string> Tags { get; set; } = tags;
    public string Grade { get; set; } = grade;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public NominationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a nomination bundle in status draft.</summary>
    public static NominationBundle NewNominationBundle(string code, string name)
    {
        return new NominationBundle(
            code,
            name,
            0.0,
            "",
            "",
            0.0,
            0,
            new List<string>(),
            "",
            0,
            "",
            NominationBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationBundleStatus next)
    {
        switch (Status)
        {
            case NominationBundleStatus.Draft:
                switch (next)
                {
                    case NominationBundleStatus.Pending:
                    case NominationBundleStatus.Closed:
                        return true;
                }
                return false;
            case NominationBundleStatus.Pending:
                return next == NominationBundleStatus.Closed;
            case NominationBundleStatus.Active:
                return next == NominationBundleStatus.Closed;
            case NominationBundleStatus.Suspended:
                switch (next)
                {
                    case NominationBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedTotal()
    {
        return Math.Round(Rate + Margin, 2);
    }

    public NominationBundle Copy()
    {
        return new NominationBundle(
            Code,
            Name,
            Rate,
            Checksum,
            Badge,
            Margin,
            DurationMinutes,
            Tags,
            Grade,
            ExpiresAt,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
