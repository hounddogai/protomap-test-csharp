namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh notice is in its life cycle.</summary>
public enum RefreshNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A refresh notice as the refresh team keeps it.</summary>
public sealed class RefreshNotice(string code, string name, string segment, int attempts, string phone, string source, string externalRef, RefreshNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public RefreshNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a refresh notice in status created.</summary>
    public static RefreshNotice NewRefreshNotice(string code, string name)
    {
        return new RefreshNotice(code, name, "", 0, "", "", "", RefreshNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshNoticeStatus next)
    {
        switch (Status)
        {
            case RefreshNoticeStatus.Created:
                switch (next)
                {
                    case RefreshNoticeStatus.Published:
                    case RefreshNoticeStatus.Retired:
                        return true;
                }
                return false;
            case RefreshNoticeStatus.Validated:
                return next == RefreshNoticeStatus.Published || next == RefreshNoticeStatus.Retired;
            case RefreshNoticeStatus.Published:
                switch (next)
                {
                    case RefreshNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public RefreshNotice Copy()
    {
        return new RefreshNotice(Code, Name, Segment, Attempts, Phone, Source, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
