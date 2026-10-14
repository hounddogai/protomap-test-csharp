namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending refresh claim is in its life cycle.</summary>
public enum PendingRefreshClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pending refresh claim and its current status.</summary>
public sealed class PendingRefreshClaim(string code, string title, string label, string grade, string externalRef, string country, List<string> tags, double rate, PendingRefreshClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string ExternalRef { get; set; } = externalRef;
    public string Country { get; set; } = country;
    public List<string> Tags { get; set; } = tags;
    public double Rate { get; set; } = rate;
    public PendingRefreshClaimStatus Status { get; set; } = status;

    /// <summary>Starts a pending refresh claim in status idle.</summary>
    public static PendingRefreshClaim NewPendingRefreshClaim(string code, string title)
    {
        return new PendingRefreshClaim(
            code,
            title,
            "",
            "",
            "",
            "",
            new List<string>(),
            0.0,
            PendingRefreshClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRefreshClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRefreshClaimStatus next)
    {
        switch (Status)
        {
            case PendingRefreshClaimStatus.Idle:
                switch (next)
                {
                    case PendingRefreshClaimStatus.Ready:
                    case PendingRefreshClaimStatus.Draining:
                        return true;
                }
                return false;
            case PendingRefreshClaimStatus.Warming:
                switch (next)
                {
                    case PendingRefreshClaimStatus.Draining:
                    case PendingRefreshClaimStatus.Stopped:
                        return true;
                }
                return false;
            case PendingRefreshClaimStatus.Ready:
                return next == PendingRefreshClaimStatus.Draining || next == PendingRefreshClaimStatus.Stopped;
            case PendingRefreshClaimStatus.Draining:
                return next == PendingRefreshClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingRefreshClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending refresh claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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
        return Rate;
    }

    public PendingRefreshClaim Copy()
    {
        return new PendingRefreshClaim(Code, Title, Label, Grade, ExternalRef, Country, Tags, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
