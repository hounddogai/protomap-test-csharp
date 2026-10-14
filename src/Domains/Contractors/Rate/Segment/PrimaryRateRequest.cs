namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary rate request is in its life cycle.</summary>
public enum PrimaryRateRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One primary rate request, with what the planners need to act on it.</summary>
public sealed class PrimaryRateRequest(string code, string label, string externalRef, string segment, int expiresAt, double cost, double minimum, string channel, int leaveDays, string costCenter, PrimaryRateRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public string CostCenter { get; set; } = costCenter;
    public PrimaryRateRequestStatus Status { get; set; } = status;

    /// <summary>Starts a primary rate request in status created.</summary>
    public static PrimaryRateRequest NewPrimaryRateRequest(string code, string label)
    {
        return new PrimaryRateRequest(code, label, "", "", 0, 0.0, 0.0, "", 0, "", PrimaryRateRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRateRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRateRequestStatus next)
    {
        switch (Status)
        {
            case PrimaryRateRequestStatus.Created:
                switch (next)
                {
                    case PrimaryRateRequestStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryRateRequestStatus.Validated:
                return next == PrimaryRateRequestStatus.Published;
            case PrimaryRateRequestStatus.Published:
                switch (next)
                {
                    case PrimaryRateRequestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryRateRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary rate request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round(Cost * Minimum, 2);
    }

    public PrimaryRateRequest Copy()
    {
        return new PrimaryRateRequest(
            Code,
            Label,
            ExternalRef,
            Segment,
            ExpiresAt,
            Cost,
            Minimum,
            Channel,
            LeaveDays,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
