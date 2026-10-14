namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default engagement digest is in its life cycle.</summary>
public enum DefaultEngagementDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default engagement digest. Its code is unique within a tenant.</summary>
public sealed class DefaultEngagementDigest(string code, string label, string name, bool archived, int batchSize, DefaultEngagementDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int BatchSize { get; set; } = batchSize;
    public DefaultEngagementDigestStatus Status { get; set; } = status;

    /// <summary>Starts a default engagement digest in status draft.</summary>
    public static DefaultEngagementDigest NewDefaultEngagementDigest(string code, string label)
    {
        return new DefaultEngagementDigest(code, label, "", false, 0, DefaultEngagementDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultEngagementDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultEngagementDigestStatus next)
    {
        switch (Status)
        {
            case DefaultEngagementDigestStatus.Draft:
                switch (next)
                {
                    case DefaultEngagementDigestStatus.Closed:
                        return true;
                }
                return false;
            case DefaultEngagementDigestStatus.Pending:
                switch (next)
                {
                    case DefaultEngagementDigestStatus.Active:
                    case DefaultEngagementDigestStatus.Closed:
                        return true;
                }
                return false;
            case DefaultEngagementDigestStatus.Active:
                return next == DefaultEngagementDigestStatus.Suspended;
            case DefaultEngagementDigestStatus.Suspended:
                return next == DefaultEngagementDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultEngagementDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default engagement digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public DefaultEngagementDigest Copy()
    {
        return new DefaultEngagementDigest(Code, Label, Name, Archived, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
