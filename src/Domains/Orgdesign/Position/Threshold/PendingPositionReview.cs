namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending position review is in its life cycle.</summary>
public enum PendingPositionReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pending position review, with what the planners need to act on it.</summary>
public sealed class PendingPositionReview(string code, string name, string label, string managerId, string externalRef, double maximum, string timezone, string salaryBand, string unit, int expiresAt, PendingPositionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public double Maximum { get; set; } = maximum;
    public string Timezone { get; set; } = timezone;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public PendingPositionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a pending position review in status created.</summary>
    public static PendingPositionReview NewPendingPositionReview(string code, string name)
    {
        return new PendingPositionReview(
            code,
            name,
            "",
            "",
            "",
            0.0,
            "",
            "",
            "",
            0,
            PendingPositionReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPositionReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPositionReviewStatus next)
    {
        switch (Status)
        {
            case PendingPositionReviewStatus.Created:
                switch (next)
                {
                    case PendingPositionReviewStatus.Published:
                        return true;
                }
                return false;
            case PendingPositionReviewStatus.Validated:
                switch (next)
                {
                    case PendingPositionReviewStatus.Published:
                        return true;
                }
                return false;
            case PendingPositionReviewStatus.Published:
                return next == PendingPositionReviewStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PendingPositionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending position review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt - Maximum, 2);
    }

    public PendingPositionReview Copy()
    {
        return new PendingPositionReview(
            Code,
            Name,
            Label,
            ManagerId,
            ExternalRef,
            Maximum,
            Timezone,
            SalaryBand,
            Unit,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
