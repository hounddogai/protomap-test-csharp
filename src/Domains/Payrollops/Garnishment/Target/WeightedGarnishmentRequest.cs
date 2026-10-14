namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted garnishment request is in its life cycle.</summary>
public enum WeightedGarnishmentRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One weighted garnishment request, with what the planners need to act on it.</summary>
public sealed class WeightedGarnishmentRequest(string code, string label, double minimum, double budget, string notes, string description, bool visible, string channel, WeightedGarnishmentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Budget { get; set; } = budget;
    public string Notes { get; set; } = notes;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public WeightedGarnishmentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted garnishment request in status proposed.</summary>
    public static WeightedGarnishmentRequest NewWeightedGarnishmentRequest(string code, string label)
    {
        return new WeightedGarnishmentRequest(
            code,
            label,
            0.0,
            0.0,
            "",
            "",
            false,
            "",
            WeightedGarnishmentRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedGarnishmentRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedGarnishmentRequestStatus next)
    {
        switch (Status)
        {
            case WeightedGarnishmentRequestStatus.Proposed:
                switch (next)
                {
                    case WeightedGarnishmentRequestStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedGarnishmentRequestStatus.Approved:
                return next == WeightedGarnishmentRequestStatus.Rejected;
            case WeightedGarnishmentRequestStatus.Rejected:
                return next == WeightedGarnishmentRequestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedGarnishmentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted garnishment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Minimum + Budget, 2);
    }

    public WeightedGarnishmentRequest Copy()
    {
        return new WeightedGarnishmentRequest(
            Code,
            Label,
            Minimum,
            Budget,
            Notes,
            Description,
            Visible,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
