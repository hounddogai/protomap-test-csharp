namespace Acme.Workforce.Compliance.Audit.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit score is in its life cycle.</summary>
public enum AuditScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a audit score and its current status.</summary>
public sealed class AuditScore(string code, string name, string skill, string externalRef, string department, string country, string checksum, double cost, int endsAt, List<string> tags, string category, AuditScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public double Cost { get; set; } = cost;
    public int EndsAt { get; set; } = endsAt;
    public List<string> Tags { get; set; } = tags;
    public string Category { get; set; } = category;
    public AuditScoreStatus Status { get; set; } = status;

    /// <summary>Starts a audit score in status idle.</summary>
    public static AuditScore NewAuditScore(string code, string name)
    {
        return new AuditScore(code, name, "", "", "", "", "", 0.0, 0, new List<string>(), "", AuditScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditScoreStatus next)
    {
        switch (Status)
        {
            case AuditScoreStatus.Idle:
                return next == AuditScoreStatus.Warming || next == AuditScoreStatus.Draining;
            case AuditScoreStatus.Warming:
                switch (next)
                {
                    case AuditScoreStatus.Ready:
                    case AuditScoreStatus.Draining:
                        return true;
                }
                return false;
            case AuditScoreStatus.Ready:
                switch (next)
                {
                    case AuditScoreStatus.Draining:
                        return true;
                }
                return false;
            case AuditScoreStatus.Draining:
                return next == AuditScoreStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AuditScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double NetLoad()
    {
        return Math.Round((double)EndsAt * Cost, 2);
    }

    public AuditScore Copy()
    {
        return new AuditScore(
            Code,
            Name,
            Skill,
            ExternalRef,
            Department,
            Country,
            Checksum,
            Cost,
            EndsAt,
            Tags,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
