namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band mapping is in its life cycle.</summary>
public enum PayBandMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pay band mapping, with what the planners need to act on it.</summary>
public sealed class PayBandMapping(string code, string label, string title, int revision, int limit, int batchSize, double threshold, int startsAt, string phone, string name, int graceDays, PayBandMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int Limit { get; set; } = limit;
    public int BatchSize { get; set; } = batchSize;
    public double Threshold { get; set; } = threshold;
    public int StartsAt { get; set; } = startsAt;
    public string Phone { get; set; } = phone;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public PayBandMappingStatus Status { get; set; } = status;

    /// <summary>Starts a pay band mapping in status open.</summary>
    public static PayBandMapping NewPayBandMapping(string code, string label)
    {
        return new PayBandMapping(code, label, "", 0, 0, 0, 0.0, 0, "", "", 0, PayBandMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandMappingStatus next)
    {
        switch (Status)
        {
            case PayBandMappingStatus.Open:
                switch (next)
                {
                    case PayBandMappingStatus.Reopened:
                        return true;
                }
                return false;
            case PayBandMappingStatus.InReview:
                switch (next)
                {
                    case PayBandMappingStatus.Reopened:
                        return true;
                }
                return false;
            case PayBandMappingStatus.Resolved:
                return next == PayBandMappingStatus.Reopened;
            case PayBandMappingStatus.Reopened:
                switch (next)
                {
                    case PayBandMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pay band mapping"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Limit + Threshold, 2);
    }

    public PayBandMapping Copy()
    {
        return new PayBandMapping(
            Code,
            Label,
            Title,
            Revision,
            Limit,
            BatchSize,
            Threshold,
            StartsAt,
            Phone,
            Name,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
