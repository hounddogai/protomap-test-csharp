namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment group is in its life cycle.</summary>
public enum GarnishmentGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A garnishment group. Its code is unique within a tenant.</summary>
public sealed class GarnishmentGroup(string code, string name, double score, string unit, double hours, string segment, string source, GarnishmentGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Unit { get; set; } = unit;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public GarnishmentGroupStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment group in status planned.</summary>
    public static GarnishmentGroup NewGarnishmentGroup(string code, string name)
    {
        return new GarnishmentGroup(code, name, 0.0, "", 0.0, "", "", GarnishmentGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentGroupStatus next)
    {
        switch (Status)
        {
            case GarnishmentGroupStatus.Planned:
                return next == GarnishmentGroupStatus.Scheduled || next == GarnishmentGroupStatus.Completed;
            case GarnishmentGroupStatus.Scheduled:
                switch (next)
                {
                    case GarnishmentGroupStatus.InProgress:
                    case GarnishmentGroupStatus.Completed:
                        return true;
                }
                return false;
            case GarnishmentGroupStatus.InProgress:
                switch (next)
                {
                    case GarnishmentGroupStatus.Completed:
                        return true;
                }
                return false;
            case GarnishmentGroupStatus.Completed:
                return next == GarnishmentGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GarnishmentGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment group"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Hours * Score, 2);
    }

    public GarnishmentGroup Copy()
    {
        return new GarnishmentGroup(Code, Name, Score, Unit, Hours, Segment, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
