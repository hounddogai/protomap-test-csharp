namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination draft is in its life cycle.</summary>
public enum NominationDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One nomination draft, with what the planners need to act on it.</summary>
public sealed class NominationDraft(string code, string label, double cost, string department, string title, double amount, string region, double discount, string managerId, NominationDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public string Department { get; set; } = department;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Region { get; set; } = region;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public NominationDraftStatus Status { get; set; } = status;

    /// <summary>Starts a nomination draft in status planned.</summary>
    public static NominationDraft NewNominationDraft(string code, string label)
    {
        return new NominationDraft(code, label, 0.0, "", "", 0.0, "", 0.0, "", NominationDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationDraftStatus next)
    {
        switch (Status)
        {
            case NominationDraftStatus.Planned:
                return next == NominationDraftStatus.Completed;
            case NominationDraftStatus.Scheduled:
                return next == NominationDraftStatus.InProgress || next == NominationDraftStatus.Completed;
            case NominationDraftStatus.InProgress:
                switch (next)
                {
                    case NominationDraftStatus.Completed:
                        return true;
                }
                return false;
            case NominationDraftStatus.Completed:
                switch (next)
                {
                    case NominationDraftStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"nomination draft"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Cost + Discount, 2);
    }

    public NominationDraft Copy()
    {
        return new NominationDraft(
            Code,
            Label,
            Cost,
            Department,
            Title,
            Amount,
            Region,
            Discount,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
