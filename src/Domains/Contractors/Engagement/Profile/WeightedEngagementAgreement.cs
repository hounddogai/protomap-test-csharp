namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted engagement agreement is in its life cycle.</summary>
public enum WeightedEngagementAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a weighted engagement agreement and its current status.</summary>
public sealed class WeightedEngagementAgreement(string code, string name, int quantity, double threshold, int updatedAt, string contactEmail, string location, string locale, WeightedEngagementAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Threshold { get; set; } = threshold;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ContactEmail { get; set; } = contactEmail;
    public string Location { get; set; } = location;
    public string Locale { get; set; } = locale;
    public WeightedEngagementAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a weighted engagement agreement in status draft.</summary>
    public static WeightedEngagementAgreement NewWeightedEngagementAgreement(string code, string name)
    {
        return new WeightedEngagementAgreement(
            code,
            name,
            0,
            0.0,
            0,
            "",
            "",
            "",
            WeightedEngagementAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedEngagementAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedEngagementAgreementStatus next)
    {
        switch (Status)
        {
            case WeightedEngagementAgreementStatus.Draft:
                switch (next)
                {
                    case WeightedEngagementAgreementStatus.Pending:
                    case WeightedEngagementAgreementStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedEngagementAgreementStatus.Pending:
                return next == WeightedEngagementAgreementStatus.Suspended;
            case WeightedEngagementAgreementStatus.Active:
                switch (next)
                {
                    case WeightedEngagementAgreementStatus.Closed:
                        return true;
                }
                return false;
            case WeightedEngagementAgreementStatus.Suspended:
                switch (next)
                {
                    case WeightedEngagementAgreementStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedEngagementAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted engagement agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Quantity + Threshold, 2);
    }

    public WeightedEngagementAgreement Copy()
    {
        return new WeightedEngagementAgreement(
            Code,
            Name,
            Quantity,
            Threshold,
            UpdatedAt,
            ContactEmail,
            Location,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
