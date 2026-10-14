namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant score is in its life cycle.</summary>
public enum EquityGrantScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A equity grant score as the equity grant team keeps it.</summary>
public sealed class EquityGrantScore(string code, string name, double hours, double budget, string externalRef, double rate, double ratio, string badge, EquityGrantScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public double Budget { get; set; } = budget;
    public string ExternalRef { get; set; } = externalRef;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public EquityGrantScoreStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant score in status new.</summary>
    public static EquityGrantScore NewEquityGrantScore(string code, string name)
    {
        return new EquityGrantScore(code, name, 0.0, 0.0, "", 0.0, 0.0, "", EquityGrantScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantScoreStatus next)
    {
        switch (Status)
        {
            case EquityGrantScoreStatus.New:
                return next == EquityGrantScoreStatus.Running;
            case EquityGrantScoreStatus.Queued:
                switch (next)
                {
                    case EquityGrantScoreStatus.Done:
                    case EquityGrantScoreStatus.Failed:
                        return true;
                }
                return false;
            case EquityGrantScoreStatus.Running:
                return next == EquityGrantScoreStatus.Failed;
            case EquityGrantScoreStatus.Done:
                return next == EquityGrantScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(EquityGrantScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Budget * Hours, 2);
    }

    public EquityGrantScore Copy()
    {
        return new EquityGrantScore(Code, Name, Hours, Budget, ExternalRef, Rate, Ratio, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
