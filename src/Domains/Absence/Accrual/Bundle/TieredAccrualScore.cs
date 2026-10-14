namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered accrual score is in its life cycle.</summary>
public enum TieredAccrualScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One tiered accrual score, with what the planners need to act on it.</summary>
public sealed class TieredAccrualScore(string code, string title, string label, double fte, string name, string externalRef, double minimum, double hours, string department, string notes, string checksum, TieredAccrualScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public double Hours { get; set; } = hours;
    public string Department { get; set; } = department;
    public string Notes { get; set; } = notes;
    public string Checksum { get; set; } = checksum;
    public TieredAccrualScoreStatus Status { get; set; } = status;

    /// <summary>Starts a tiered accrual score in status idle.</summary>
    public static TieredAccrualScore NewTieredAccrualScore(string code, string title)
    {
        return new TieredAccrualScore(
            code,
            title,
            "",
            0.0,
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            "",
            TieredAccrualScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAccrualScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAccrualScoreStatus next)
    {
        switch (Status)
        {
            case TieredAccrualScoreStatus.Idle:
                switch (next)
                {
                    case TieredAccrualScoreStatus.Warming:
                        return true;
                }
                return false;
            case TieredAccrualScoreStatus.Warming:
                switch (next)
                {
                    case TieredAccrualScoreStatus.Ready:
                        return true;
                }
                return false;
            case TieredAccrualScoreStatus.Ready:
                switch (next)
                {
                    case TieredAccrualScoreStatus.Draining:
                    case TieredAccrualScoreStatus.Stopped:
                        return true;
                }
                return false;
            case TieredAccrualScoreStatus.Draining:
                switch (next)
                {
                    case TieredAccrualScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredAccrualScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered accrual score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Hours * Minimum, 2);
    }

    public TieredAccrualScore Copy()
    {
        return new TieredAccrualScore(
            Code,
            Title,
            Label,
            Fte,
            Name,
            ExternalRef,
            Minimum,
            Hours,
            Department,
            Notes,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
