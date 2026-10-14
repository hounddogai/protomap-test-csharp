namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default feedback reservation is in its life cycle.</summary>
public enum DefaultFeedbackReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default feedback reservation and its current status.</summary>
public sealed class DefaultFeedbackReservation(string code, string label, Dictionary<string, string> attributes, string managerId, int tenureMonths, string salaryBand, string timezone, int quantity, string contactEmail, string description, DefaultFeedbackReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public string SalaryBand { get; set; } = salaryBand;
    public string Timezone { get; set; } = timezone;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public string Description { get; set; } = description;
    public DefaultFeedbackReservationStatus Status { get; set; } = status;

    /// <summary>Starts a default feedback reservation in status planned.</summary>
    public static DefaultFeedbackReservation NewDefaultFeedbackReservation(string code, string label)
    {
        return new DefaultFeedbackReservation(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            "",
            0,
            "",
            "",
            DefaultFeedbackReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultFeedbackReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultFeedbackReservationStatus next)
    {
        switch (Status)
        {
            case DefaultFeedbackReservationStatus.Planned:
                switch (next)
                {
                    case DefaultFeedbackReservationStatus.Completed:
                        return true;
                }
                return false;
            case DefaultFeedbackReservationStatus.Scheduled:
                switch (next)
                {
                    case DefaultFeedbackReservationStatus.InProgress:
                    case DefaultFeedbackReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultFeedbackReservationStatus.InProgress:
                switch (next)
                {
                    case DefaultFeedbackReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultFeedbackReservationStatus.Completed:
                return next == DefaultFeedbackReservationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultFeedbackReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default feedback reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double EffectiveTotal()
    {
        return Math.Round((double)Quantity * (double)TenureMonths, 2);
    }

    public DefaultFeedbackReservation Copy()
    {
        return new DefaultFeedbackReservation(
            Code,
            Label,
            Attributes,
            ManagerId,
            TenureMonths,
            SalaryBand,
            Timezone,
            Quantity,
            ContactEmail,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
