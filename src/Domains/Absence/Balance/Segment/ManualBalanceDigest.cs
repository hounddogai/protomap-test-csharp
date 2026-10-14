namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual balance digest is in its life cycle.</summary>
public enum ManualBalanceDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual balance digest as the balance team keeps it.</summary>
public sealed class ManualBalanceDigest(string code, string title, string skill, string segment, int expiresAt, List<string> tags, ManualBalanceDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public List<string> Tags { get; set; } = tags;
    public ManualBalanceDigestStatus Status { get; set; } = status;

    /// <summary>Starts a manual balance digest in status idle.</summary>
    public static ManualBalanceDigest NewManualBalanceDigest(string code, string title)
    {
        return new ManualBalanceDigest(code, title, "", "", 0, new List<string>(), ManualBalanceDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBalanceDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBalanceDigestStatus next)
    {
        switch (Status)
        {
            case ManualBalanceDigestStatus.Idle:
                return next == ManualBalanceDigestStatus.Warming || next == ManualBalanceDigestStatus.Draining;
            case ManualBalanceDigestStatus.Warming:
                switch (next)
                {
                    case ManualBalanceDigestStatus.Ready:
                    case ManualBalanceDigestStatus.Draining:
                        return true;
                }
                return false;
            case ManualBalanceDigestStatus.Ready:
                return next == ManualBalanceDigestStatus.Draining || next == ManualBalanceDigestStatus.Stopped;
            case ManualBalanceDigestStatus.Draining:
                return next == ManualBalanceDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualBalanceDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual balance digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual balance digest"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public ManualBalanceDigest Copy()
    {
        return new ManualBalanceDigest(Code, Title, Skill, Segment, ExpiresAt, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
