namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan reservation is in its life cycle.</summary>
public enum ActionPlanReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A action plan reservation as the action plan team keeps it.</summary>
public sealed class ActionPlanReservation(string code, string title, bool pinned, string contactEmail, string location, string country, int endsAt, string reviewer, double amount, ActionPlanReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public string Location { get; set; } = location;
    public string Country { get; set; } = country;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public double Amount { get; set; } = amount;
    public ActionPlanReservationStatus Status { get; set; } = status;

    /// <summary>Starts a action plan reservation in status planned.</summary>
    public static ActionPlanReservation NewActionPlanReservation(string code, string title)
    {
        return new ActionPlanReservation(
            code,
            title,
            false,
            "",
            "",
            "",
            0,
            "",
            0.0,
            ActionPlanReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanReservationStatus next)
    {
        switch (Status)
        {
            case ActionPlanReservationStatus.Planned:
                switch (next)
                {
                    case ActionPlanReservationStatus.Scheduled:
                    case ActionPlanReservationStatus.InProgress:
                        return true;
                }
                return false;
            case ActionPlanReservationStatus.Scheduled:
                switch (next)
                {
                    case ActionPlanReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanReservationStatus.InProgress:
                return next == ActionPlanReservationStatus.Completed || next == ActionPlanReservationStatus.Cancelled;
            case ActionPlanReservationStatus.Completed:
                switch (next)
                {
                    case ActionPlanReservationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Amount + (double)EndsAt, 2);
    }

    public ActionPlanReservation Copy()
    {
        return new ActionPlanReservation(
            Code,
            Title,
            Pinned,
            ContactEmail,
            Location,
            Country,
            EndsAt,
            Reviewer,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
