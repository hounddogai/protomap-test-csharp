namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment slot is in its life cycle.</summary>
public enum GarnishmentSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One garnishment slot, with what the planners need to act on it.</summary>
public sealed class GarnishmentSlot(string code, string title, string name, double maximum, string managerId, string jobCode, string notes, bool approved, double ceiling, int graceDays, GarnishmentSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public bool Approved { get; set; } = approved;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public GarnishmentSlotStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment slot in status open.</summary>
    public static GarnishmentSlot NewGarnishmentSlot(string code, string title)
    {
        return new GarnishmentSlot(code, title, "", 0.0, "", "", "", false, 0.0, 0, GarnishmentSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentSlotStatus next)
    {
        switch (Status)
        {
            case GarnishmentSlotStatus.Open:
                switch (next)
                {
                    case GarnishmentSlotStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentSlotStatus.InReview:
                return next == GarnishmentSlotStatus.Reopened;
            case GarnishmentSlotStatus.Resolved:
                switch (next)
                {
                    case GarnishmentSlotStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentSlotStatus.Reopened:
                switch (next)
                {
                    case GarnishmentSlotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment slot"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ceiling - Maximum, 2);
    }

    public GarnishmentSlot Copy()
    {
        return new GarnishmentSlot(
            Code,
            Title,
            Name,
            Maximum,
            ManagerId,
            JobCode,
            Notes,
            Approved,
            Ceiling,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
