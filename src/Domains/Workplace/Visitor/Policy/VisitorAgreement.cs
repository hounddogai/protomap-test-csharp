namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor agreement is in its life cycle.</summary>
public enum VisitorAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a visitor agreement and its current status.</summary>
public sealed class VisitorAgreement(string code, string name, int durationMinutes, string contactEmail, int expiresAt, double budget, bool active, int quantity, string location, string timezone, VisitorAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public bool Active { get; set; } = active;
    public int Quantity { get; set; } = quantity;
    public string Location { get; set; } = location;
    public string Timezone { get; set; } = timezone;
    public VisitorAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a visitor agreement in status open.</summary>
    public static VisitorAgreement NewVisitorAgreement(string code, string name)
    {
        return new VisitorAgreement(code, name, 0, "", 0, 0.0, false, 0, "", "", VisitorAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorAgreementStatus next)
    {
        switch (Status)
        {
            case VisitorAgreementStatus.Open:
                switch (next)
                {
                    case VisitorAgreementStatus.Resolved:
                    case VisitorAgreementStatus.Archived:
                        return true;
                }
                return false;
            case VisitorAgreementStatus.InReview:
                return next == VisitorAgreementStatus.Archived;
            case VisitorAgreementStatus.Resolved:
                switch (next)
                {
                    case VisitorAgreementStatus.Archived:
                        return true;
                }
                return false;
            case VisitorAgreementStatus.Reopened:
                switch (next)
                {
                    case VisitorAgreementStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget + (double)Quantity, 2);
    }

    public VisitorAgreement Copy()
    {
        return new VisitorAgreement(
            Code,
            Name,
            DurationMinutes,
            ContactEmail,
            ExpiresAt,
            Budget,
            Active,
            Quantity,
            Location,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
