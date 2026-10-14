namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band group is in its life cycle.</summary>
public enum PayBandGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pay band group as the pay band team keeps it.</summary>
public sealed class PayBandGroup(string code, string label, string reviewer, string channel, int sequence, string managerId, int tenureMonths, string contactEmail, int batchSize, Dictionary<string, string> attributes, PayBandGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Channel { get; set; } = channel;
    public int Sequence { get; set; } = sequence;
    public string ManagerId { get; set; } = managerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public PayBandGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pay band group in status open.</summary>
    public static PayBandGroup NewPayBandGroup(string code, string label)
    {
        return new PayBandGroup(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            "",
            0,
            new Dictionary<string, string>(),
            PayBandGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandGroupStatus next)
    {
        switch (Status)
        {
            case PayBandGroupStatus.Open:
                return next == PayBandGroupStatus.InReview || next == PayBandGroupStatus.Resolved;
            case PayBandGroupStatus.InReview:
                switch (next)
                {
                    case PayBandGroupStatus.Resolved:
                        return true;
                }
                return false;
            case PayBandGroupStatus.Resolved:
                switch (next)
                {
                    case PayBandGroupStatus.Reopened:
                    case PayBandGroupStatus.Archived:
                        return true;
                }
                return false;
            case PayBandGroupStatus.Reopened:
                switch (next)
                {
                    case PayBandGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)BatchSize * (double)TenureMonths, 2);
    }

    public PayBandGroup Copy()
    {
        return new PayBandGroup(
            Code,
            Label,
            Reviewer,
            Channel,
            Sequence,
            ManagerId,
            TenureMonths,
            ContactEmail,
            BatchSize,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
