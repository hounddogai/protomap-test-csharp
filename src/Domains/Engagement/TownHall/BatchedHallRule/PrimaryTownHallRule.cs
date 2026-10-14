namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary town hall rule is in its life cycle.</summary>
public enum PrimaryTownHallRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a primary town hall rule and its current status.</summary>
public sealed class PrimaryTownHallRule(string code, string title, double hours, double rating, int quantity, double budget, double margin, string channel, string summary, int attempts, PrimaryTownHallRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public double Rating { get; set; } = rating;
    public int Quantity { get; set; } = quantity;
    public double Budget { get; set; } = budget;
    public double Margin { get; set; } = margin;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public PrimaryTownHallRuleStatus Status { get; set; } = status;

    /// <summary>Starts a primary town hall rule in status proposed.</summary>
    public static PrimaryTownHallRule NewPrimaryTownHallRule(string code, string title)
    {
        return new PrimaryTownHallRule(
            code,
            title,
            0.0,
            0.0,
            0,
            0.0,
            0.0,
            "",
            "",
            0,
            PrimaryTownHallRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTownHallRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTownHallRuleStatus next)
    {
        switch (Status)
        {
            case PrimaryTownHallRuleStatus.Proposed:
                switch (next)
                {
                    case PrimaryTownHallRuleStatus.Approved:
                    case PrimaryTownHallRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PrimaryTownHallRuleStatus.Approved:
                return next == PrimaryTownHallRuleStatus.Rejected;
            case PrimaryTownHallRuleStatus.Rejected:
                switch (next)
                {
                    case PrimaryTownHallRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryTownHallRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary town hall rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Hours * Rating, 2);
    }

    public PrimaryTownHallRule Copy()
    {
        return new PrimaryTownHallRule(
            Code,
            Title,
            Hours,
            Rating,
            Quantity,
            Budget,
            Margin,
            Channel,
            Summary,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
