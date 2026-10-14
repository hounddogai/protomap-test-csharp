namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient cost model summary is in its life cycle.</summary>
public enum LenientCostModelSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient cost model summary, with what the planners need to act on it.</summary>
public sealed class LenientCostModelSummary(string code, string name, bool archived, double weight, double hours, double ceiling, double budget, string badge, string costCenter, string team, LenientCostModelSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public double Budget { get; set; } = budget;
    public string Badge { get; set; } = badge;
    public string CostCenter { get; set; } = costCenter;
    public string Team { get; set; } = team;
    public LenientCostModelSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient cost model summary in status requested.</summary>
    public static LenientCostModelSummary NewLenientCostModelSummary(string code, string name)
    {
        return new LenientCostModelSummary(
            code,
            name,
            false,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            "",
            "",
            LenientCostModelSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientCostModelSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientCostModelSummaryStatus next)
    {
        switch (Status)
        {
            case LenientCostModelSummaryStatus.Requested:
                switch (next)
                {
                    case LenientCostModelSummaryStatus.Expired:
                        return true;
                }
                return false;
            case LenientCostModelSummaryStatus.Granted:
                switch (next)
                {
                    case LenientCostModelSummaryStatus.Revoked:
                    case LenientCostModelSummaryStatus.Expired:
                        return true;
                }
                return false;
            case LenientCostModelSummaryStatus.Revoked:
                return next == LenientCostModelSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientCostModelSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient cost model summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Ceiling * Weight, 2);
    }

    public LenientCostModelSummary Copy()
    {
        return new LenientCostModelSummary(
            Code,
            Name,
            Archived,
            Weight,
            Hours,
            Ceiling,
            Budget,
            Badge,
            CostCenter,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
