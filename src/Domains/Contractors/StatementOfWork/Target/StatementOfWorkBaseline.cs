namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work baseline is in its life cycle.</summary>
public enum StatementOfWorkBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One statement of work baseline, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkBaseline(string code, string label, int startsAt, int graceDays, string externalRef, int endsAt, double fte, double ceiling, double hours, int sequence, StatementOfWorkBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public int GraceDays { get; set; } = graceDays;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public double Hours { get; set; } = hours;
    public int Sequence { get; set; } = sequence;
    public StatementOfWorkBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work baseline in status created.</summary>
    public static StatementOfWorkBaseline NewStatementOfWorkBaseline(string code, string label)
    {
        return new StatementOfWorkBaseline(
            code,
            label,
            0,
            0,
            "",
            0,
            0.0,
            0.0,
            0.0,
            0,
            StatementOfWorkBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkBaselineStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkBaselineStatus.Created:
                return next == StatementOfWorkBaselineStatus.Published || next == StatementOfWorkBaselineStatus.Retired;
            case StatementOfWorkBaselineStatus.Validated:
                return next == StatementOfWorkBaselineStatus.Published || next == StatementOfWorkBaselineStatus.Retired;
            case StatementOfWorkBaselineStatus.Published:
                return next == StatementOfWorkBaselineStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt * Ceiling, 2);
    }

    public StatementOfWorkBaseline Copy()
    {
        return new StatementOfWorkBaseline(
            Code,
            Label,
            StartsAt,
            GraceDays,
            ExternalRef,
            EndsAt,
            Fte,
            Ceiling,
            Hours,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
