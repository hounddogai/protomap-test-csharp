namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday budget is in its life cycle.</summary>
public enum HolidayBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One holiday budget, with what the planners need to act on it.</summary>
public sealed class HolidayBudget(string code, string label, double rate, string phone, int quantity, string jobCode, double budget, int startsAt, bool archived, string contactEmail, string summary, HolidayBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Phone { get; set; } = phone;
    public int Quantity { get; set; } = quantity;
    public string JobCode { get; set; } = jobCode;
    public double Budget { get; set; } = budget;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public string ContactEmail { get; set; } = contactEmail;
    public string Summary { get; set; } = summary;
    public HolidayBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a holiday budget in status open.</summary>
    public static HolidayBudget NewHolidayBudget(string code, string label)
    {
        return new HolidayBudget(code, label, 0.0, "", 0, "", 0.0, 0, false, "", "", HolidayBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayBudgetStatus next)
    {
        switch (Status)
        {
            case HolidayBudgetStatus.Open:
                return next == HolidayBudgetStatus.Resolved || next == HolidayBudgetStatus.Reopened;
            case HolidayBudgetStatus.InReview:
                return next == HolidayBudgetStatus.Resolved;
            case HolidayBudgetStatus.Resolved:
                switch (next)
                {
                    case HolidayBudgetStatus.Reopened:
                    case HolidayBudgetStatus.Archived:
                        return true;
                }
                return false;
            case HolidayBudgetStatus.Reopened:
                switch (next)
                {
                    case HolidayBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Quantity - Rate, 2);
    }

    public HolidayBudget Copy()
    {
        return new HolidayBudget(
            Code,
            Label,
            Rate,
            Phone,
            Quantity,
            JobCode,
            Budget,
            StartsAt,
            Archived,
            ContactEmail,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
