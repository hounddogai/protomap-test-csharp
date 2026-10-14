namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant digest is in its life cycle.</summary>
public enum EquityGrantDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a equity grant digest and its current status.</summary>
public sealed class EquityGrantDigest(string code, string label, string channel, string department, string jobCode, string notes, string contactEmail, int durationMinutes, double ratio, string costCenter, int retries, EquityGrantDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ratio { get; set; } = ratio;
    public string CostCenter { get; set; } = costCenter;
    public int Retries { get; set; } = retries;
    public EquityGrantDigestStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant digest in status open.</summary>
    public static EquityGrantDigest NewEquityGrantDigest(string code, string label)
    {
        return new EquityGrantDigest(code, label, "", "", "", "", "", 0, 0.0, "", 0, EquityGrantDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantDigestStatus next)
    {
        switch (Status)
        {
            case EquityGrantDigestStatus.Open:
                switch (next)
                {
                    case EquityGrantDigestStatus.InReview:
                    case EquityGrantDigestStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantDigestStatus.InReview:
                return next == EquityGrantDigestStatus.Resolved;
            case EquityGrantDigestStatus.Resolved:
                return next == EquityGrantDigestStatus.Reopened || next == EquityGrantDigestStatus.Archived;
            case EquityGrantDigestStatus.Reopened:
                switch (next)
                {
                    case EquityGrantDigestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"equity grant digest"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ratio + (double)Retries, 2);
    }

    public EquityGrantDigest Copy()
    {
        return new EquityGrantDigest(
            Code,
            Label,
            Channel,
            Department,
            JobCode,
            Notes,
            ContactEmail,
            DurationMinutes,
            Ratio,
            CostCenter,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
