namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary succession plan request is in its life cycle.</summary>
public enum PrimarySuccessionPlanRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a primary succession plan request and its current status.</summary>
public sealed class PrimarySuccessionPlanRequest(string code, string title, double minimum, double rating, string timezone, double cost, string description, PrimarySuccessionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public string Description { get; set; } = description;
    public PrimarySuccessionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a primary succession plan request in status draft.</summary>
    public static PrimarySuccessionPlanRequest NewPrimarySuccessionPlanRequest(string code, string title)
    {
        return new PrimarySuccessionPlanRequest(
            code,
            title,
            0.0,
            0.0,
            "",
            0.0,
            "",
            PrimarySuccessionPlanRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySuccessionPlanRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySuccessionPlanRequestStatus next)
    {
        switch (Status)
        {
            case PrimarySuccessionPlanRequestStatus.Draft:
                switch (next)
                {
                    case PrimarySuccessionPlanRequestStatus.Pending:
                    case PrimarySuccessionPlanRequestStatus.Active:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanRequestStatus.Pending:
                switch (next)
                {
                    case PrimarySuccessionPlanRequestStatus.Suspended:
                    case PrimarySuccessionPlanRequestStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanRequestStatus.Active:
                switch (next)
                {
                    case PrimarySuccessionPlanRequestStatus.Suspended:
                    case PrimarySuccessionPlanRequestStatus.Closed:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanRequestStatus.Suspended:
                switch (next)
                {
                    case PrimarySuccessionPlanRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimarySuccessionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary succession plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Cost - Minimum, 2);
    }

    public PrimarySuccessionPlanRequest Copy()
    {
        return new PrimarySuccessionPlanRequest(Code, Title, Minimum, Rating, Timezone, Cost, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
